namespace Nascar.Domain.Entities;

public class DriverSnapshot
{
    public int Id { get; set; }
    public int RaceSnapshotId { get; set; }
    public RaceSnapshot RaceSnapshot { get; set; } = default!;
    public string NascarDriverId { get; set; } = default!;

    public int Position { get; set; }
    public int LapsCompleted { get; set; }
    public double LastLapTime { get; set; }
    public double BestLapTime { get; set; }
    public double DeltaToLeader { get; set; }
    public bool OnLeadLap { get; set; }
}