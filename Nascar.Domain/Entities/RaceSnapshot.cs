namespace Nascar.Domain.Entities;

public class RaceSnapshot
{
    public int Id { get; set; }
    public string EventId { get; set; } = default!;
    public DateTime CapturedAtUtc { get; set; }
    public int Lap { get; set; }
    public ICollection<DriverSnapshot> DriverSnapshots { get; set; } = new List<DriverSnapshot>();
}