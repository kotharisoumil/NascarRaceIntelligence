using Microsoft.ML;
using Microsoft.ML.Data;
using Nascar.Infrastructure.Data;
using Nascar.Domain.Entities;

namespace Nascar.Api.Services
{
    // Input to the model at prediction time
    public class PredictionFeatures
    {
        public float LapsCompleted { get; set; }
        public float Position { get; set; }
        public float DeltaToLeader { get; set; }
        public float LastLapTime { get; set; }
        public float BestLapTime { get; set; }
    }

    // Row used while training (includes Label)
    public class PredictionLabel
    {
        public bool Label { get; set; }   // true if final position <= 5

        public float LapsCompleted { get; set; }
        public float Position { get; set; }
        public float DeltaToLeader { get; set; }
        public float LastLapTime { get; set; }
        public float BestLapTime { get; set; }
    }

    // Output from the model
    public class PredictionOutput
    {
        public bool PredictedLabel { get; set; }

        [ColumnName("Probability")]
        public float Probability { get; set; }

        public float Score { get; set; }
    }

    /// <summary>
    /// Trains a FastTree binary classifier on real race snapshots
    /// and exposes a method to get top‑5 probability for a driver.
    /// </summary>
    public class PredictionService
    {
        private readonly MLContext _ml;
        private readonly NascarDbContext _db;

        private ITransformer? _model;
        private PredictionEngine<PredictionFeatures, PredictionOutput>? _engine;
        private readonly object _lock = new();

        // Optional: store model on disk so you don’t retrain every run
        private const string ModelPath = "Models/top5_model.zip";

        public PredictionService(MLContext ml, NascarDbContext db)
        {
            _ml = ml;
            _db = db;
        }

        /// <summary>
        /// Main method the rest of the app will call.
        /// </summary>
        public float PredictTop5Probability(PredictionFeatures features)
        {
            EnsureModel();
            if (_engine == null)
                return 0.0f;

            var result = _engine.Predict(features);
            return result.Probability;
        }

        /// <summary>
        /// Ensure model is loaded or trained (called lazily).
        /// </summary>
        private void EnsureModel()
        {
            if (_engine != null)
                return;

            lock (_lock)
            {
                if (_engine != null)
                    return;

                // Try load from disk first
                if (System.IO.File.Exists(ModelPath))
                {
                    using var fs = System.IO.File.OpenRead(ModelPath);
                    _model = _ml.Model.Load(fs, out _);
                    _engine = _ml.Model.CreatePredictionEngine<PredictionFeatures, PredictionOutput>(_model);
                    return;
                }

                // Train a new model from real snapshots if no model exists
                var trainingRows = BuildTrainingDataFromRealSnapshots().ToList();
                if (!trainingRows.Any())
                {
                    // No data yet: leave engine null (probability will return 0)
                    return;
                }

                var trainData = _ml.Data.LoadFromEnumerable(trainingRows);

                var pipeline = _ml.Transforms
                    .Concatenate("Features",
                        nameof(PredictionLabel.LapsCompleted),
                        nameof(PredictionLabel.Position),
                        nameof(PredictionLabel.DeltaToLeader),
                        nameof(PredictionLabel.LastLapTime),
                        nameof(PredictionLabel.BestLapTime))
                    .Append(_ml.BinaryClassification.Trainers.FastTree());

                _model = pipeline.Fit(trainData);

                // Save model for next run
                System.IO.Directory.CreateDirectory("Models");
                using (var fs = System.IO.File.Create(ModelPath))
                {
                    _ml.Model.Save(_model, trainData.Schema, fs);
                }

                _engine = _ml.Model.CreatePredictionEngine<PredictionFeatures, PredictionOutput>(_model);
            }
        }

        /// <summary>
        /// Build training rows from REAL final snapshots in the database.
        /// Each row = one driver’s final status in a race.
        /// Label = true if final position <= 5.
        /// </summary>
        private IEnumerable<PredictionLabel> BuildTrainingDataFromRealSnapshots()
        {
            // For each EventId, take the latest snapshot as "final"
            var lastSnapshots = _db.RaceSnapshots
                .GroupBy(r => r.EventId)
                .Select(g => g.OrderByDescending(r => r.CapturedAtUtc).First())
                .ToList();

            var result = new List<PredictionLabel>();

            foreach (var snap in lastSnapshots)
            {
                // Ensure driver snapshots are loaded
                _db.Entry(snap)
                   .Collection(s => s.DriverSnapshots)
                   .Load();

                foreach (var d in snap.DriverSnapshots)
                {
                    // Skip obviously incomplete records
                    if (d.LapsCompleted <= 0 || d.Position <= 0)
                        continue;

                    result.Add(new PredictionLabel
                    {
                        Label = d.Position <= 5,
                        LapsCompleted = d.LapsCompleted,
                        Position = d.Position,
                        DeltaToLeader = (float)d.DeltaToLeader,
                        LastLapTime = (float)d.LastLapTime,
                        BestLapTime = (float)d.BestLapTime
                    });
                }
            }

            return result;
        }
    }
}