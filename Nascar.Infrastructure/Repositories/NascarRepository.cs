using Microsoft.EntityFrameworkCore;
using Nascar.Domain.Entities;
using Nascar.Infrastructure.Data;

namespace Nascar.Infrastructure.Repositories;

public class NascarRepository : INascarRepository
{
    private readonly NascarDbContext _db;

    public NascarRepository(NascarDbContext db) => _db = db;

    public async Task SaveSnapshotAsync(RaceSnapshot snapshot, CancellationToken ct = default)
    {
        _db.RaceSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(ct);
    }

    public Task<RaceSnapshot?> GetLatestSnapshotAsync(string eventId, CancellationToken ct = default) =>
        _db.RaceSnapshots
           .Where(r => r.EventId == eventId)
           .OrderByDescending(r => r.CapturedAtUtc)
           .Include(r => r.DriverSnapshots)
           .FirstOrDefaultAsync(ct);

    public Task<List<RaceSnapshot>> GetSnapshotsForEventAsync(string eventId, CancellationToken ct = default) =>
        _db.RaceSnapshots
           .Where(r => r.EventId == eventId)
           .OrderBy(r => r.CapturedAtUtc)
           .Include(r => r.DriverSnapshots)
           .ToListAsync(ct);
}