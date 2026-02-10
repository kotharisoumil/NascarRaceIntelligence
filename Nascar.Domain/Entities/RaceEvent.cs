namespace Nascar.Domain.Entities;

public class RaceEvent
{
    public int Id { get; set; }
    public string EventId { get; set; } = default!;   // NASCAR event id, e.g., "5273"
    public string SeriesId { get; set; } = "1";       // 1 = Cup
    public string Name { get; set; } = default!;
    public DateTime StartTimeUtc { get; set; }
    public string TrackName { get; set; } = default!;
}