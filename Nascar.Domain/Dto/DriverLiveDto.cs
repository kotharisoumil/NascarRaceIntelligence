namespace Nascar.Domain.Dto;

public class DriverLiveDto
{
    public string DriverId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string CarNumber { get; set; } = default!;
    public int Position { get; set; }
    public int LapsCompleted { get; set; }
    public double LastLapTime { get; set; }
    public double BestLapTime { get; set; }
    public double DeltaToLeader { get; set; }
    public bool OnLeadLap { get; set; }
    public float Top5Probability { get; set; }
}