using FuelPriceTracker.Data;
using FuelPriceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FuelPriceTracker.Repositories;

public sealed class PriceSnapshotRepository(FuelPriceDbContext dbContext) : IPriceSnapshotRepository
{
    public async Task<Dictionary<string, PriceSnapshot>> GetLatestPerStationAsync(IEnumerable<string> stationIds, CancellationToken cancellationToken)
    {
        var ids = stationIds.Distinct().ToList();

        return await dbContext.PriceSnapshots
            .Where(x => ids.Contains(x.StationId))
            .GroupBy(x => x.StationId)
            .Select(g => g.OrderByDescending(x => x.TimestampUtc).First())
            .ToDictionaryAsync(x => x.StationId, cancellationToken);
    }

    public async Task SaveSnapshotsAsync(IEnumerable<PriceSnapshot> snapshots, IEnumerable<PriceChange> changes, CancellationToken cancellationToken)
    {
        var snapshotList = snapshots.ToList();
        var changeList = changes.ToList();

        if (snapshotList.Count > 0)
        {
            await dbContext.PriceSnapshots.AddRangeAsync(snapshotList, cancellationToken);
        }

        if (changeList.Count > 0)
        {
            await dbContext.PriceChanges.AddRangeAsync(changeList, cancellationToken);
        }

        if (snapshotList.Count > 0 || changeList.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<int> CountPriceChangesForDayAsync(DateOnly day, CancellationToken cancellationToken)
    {
        var start = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        return dbContext.PriceChanges.CountAsync(x => x.TimestampUtc >= start && x.TimestampUtc < end, cancellationToken);
    }
}
