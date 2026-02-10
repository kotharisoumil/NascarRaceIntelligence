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
    public int lap_number { get; set; }
    public LiveFlagState flag_state { get; set; } = new();
    public List<LiveVehicle> vehicles { get; set; } = new();
}

public class LiveFlagState
{
    public string flag_status { get; set; } = string.Empty;
}

public class LiveVehicle
{
    public string vehicle_id { get; set; } = string.Empty;
    public string driver_name { get; set; } = string.Empty;
    public string car_number { get; set; } = string.Empty;
    public int running_position { get; set; }
    public int laps_completed { get; set; }
    public double last_lap_time { get; set; }
    public double best_lap_time { get; set; }
    public double interval { get; set; }
}