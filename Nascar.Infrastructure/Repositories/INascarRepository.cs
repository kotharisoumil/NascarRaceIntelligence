using Nascar.Domain.Entities;

namespace Nascar.Infrastructure.Repositories;

public interface INascarRepository
{
    Task SaveSnapshotAsync(RaceSnapshot snapshot, CancellationToken ct = default);
    Task<RaceSnapshot?> GetLatestSnapshotAsync(string eventId, CancellationToken ct = default);
    Task<List<RaceSnapshot>> GetSnapshotsForEventAsync(string eventId, CancellationToken ct = default);
}