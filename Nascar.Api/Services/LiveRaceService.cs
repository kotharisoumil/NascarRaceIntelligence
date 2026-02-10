using Nascar.Api.Clients;
using Nascar.Domain.Dto;
using Nascar.Domain.Entities;
using Nascar.Infrastructure.Repositories;

namespace Nascar.Api.Services;

public class LiveRaceService
{
    private readonly NascarLiveFeedClient _client;
    private readonly INascarRepository _repo;
    private readonly PredictionService _prediction;

    public LiveRaceService(
        NascarLiveFeedClient client,
        INascarRepository repo,
        PredictionService prediction)
    {
        _client = client;
        _repo = repo;
        _prediction = prediction;
    }

    public async Task<IReadOnlyList<DriverLiveDto>> GetLiveDriversAsync(
        string seriesId,
        string eventId,
        CancellationToken ct = default)
    {
        var feed = await _client.GetLiveFeedAsync(seriesId, eventId, ct);
        if (feed == null) return Array.Empty<DriverLiveDto>();

        var snapshot = new RaceSnapshot
        {
            EventId = eventId,
            CapturedAtUtc = DateTime.UtcNow,
            Lap = feed.lap_number,
            DriverSnapshots = feed.vehicles.Select(v => new DriverSnapshot
            {
                NascarDriverId = v.vehicle_id,
                Position = v.running_position,
                LapsCompleted = v.laps_completed,
                LastLapTime = v.last_lap_time,
                BestLapTime = v.best_lap_time,
                DeltaToLeader = v.interval,
                OnLeadLap = v.interval == 0
            }).ToList()
        };

        await _repo.SaveSnapshotAsync(snapshot, ct);

        var dtos = new List<DriverLiveDto>();
        foreach (var v in feed.vehicles.OrderBy(v => v.running_position))
        {
            var features = new PredictionFeatures
            {
                LapsCompleted = v.laps_completed,
                Position = v.running_position,
                DeltaToLeader = (float)v.interval,
                LastLapTime = (float)v.last_lap_time,
                BestLapTime = (float)v.best_lap_time
            };

            var prob = _prediction.PredictTop5Probability(features);

            dtos.Add(new DriverLiveDto
            {
                DriverId = v.vehicle_id,
                Name = v.driver_name,
                CarNumber = v.car_number,
                Position = v.running_position,
                LapsCompleted = v.laps_completed,
                LastLapTime = v.last_lap_time,
                BestLapTime = v.best_lap_time,
                DeltaToLeader = v.interval,
                OnLeadLap = v.interval == 0,
                Top5Probability = prob
            });
        }

        return dtos;
    }

    public async Task<IReadOnlyList<DriverSnapshot>> GetDriverHistoryAsync(
        string eventId, string driverId, CancellationToken ct = default)
    {
        var snapshots = await _repo.GetSnapshotsForEventAsync(eventId, ct);
        return snapshots
            .SelectMany(s => s.DriverSnapshots)
            .Where(d => d.NascarDriverId == driverId)
            .OrderBy(d => d.RaceSnapshot.CapturedAtUtc)
            .ToList();
    }
}