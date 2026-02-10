using System.Net.Http.Json;

namespace Nascar.Api.Clients;

public class NascarLiveFeedClient
{
    private readonly HttpClient _http;

    public NascarLiveFeedClient(HttpClient http) => _http = http;

    public async Task<LiveFeedRoot?> GetLiveFeedAsync(string seriesId, string eventId, CancellationToken ct = default)
    {
        // Example: series_1/5273/live_feed.json
        var url = $"live/feeds/series_{seriesId}/{eventId}/live_feed.json";
        return await _http.GetFromJsonAsync<LiveFeedRoot>(url, ct);
    }
}

// Type the subset you need, based on real feed structure
public class LiveFeedRoot
{
    public LiveFlagState FlagState { get; set; } = new();
    public List<LiveVehicle> Vehicles { get; set; } = new();
    public int LapNumber { get; set; }
}

public class LiveFlagState
{
    public string FlagStatus { get; set; } = default!; // GREEN, YELLOW, etc.
}

public class LiveVehicle
{
    public string VehicleId { get; set; } = default!; // driver id
    public string DriverName { get; set; } = default!;
    public string CarNumber { get; set; } = default!;
    public int RunningPosition { get; set; }
    public int LapsCompleted { get; set; }
    public double LastLapTime { get; set; }
    public double BestLapTime { get; set; }
    public double Interval { get; set; } // vs leader
}