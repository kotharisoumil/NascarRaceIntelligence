namespace Nascar.Domain.Entities;

public class Driver
{
    public int Id { get; set; }
    public string NascarDriverId { get; set; } = default!;  // from feed
    public string Name { get; set; } = default!;
    public string CarNumber { get; set; } = default!;
    public string Manufacturer { get; set; } = default!;
    public string Team { get; set; } = default!;
}