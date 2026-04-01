using FuelPriceTracker.Models;

namespace FuelPriceTracker.Repositories;

public interface IPriceSnapshotRepository
{
    Task<Dictionary<string, PriceSnapshot>> GetLatestPerStationAsync(IEnumerable<string> stationIds, CancellationToken cancellationToken);
    Task SaveSnapshotsAsync(IEnumerable<PriceSnapshot> snapshots, IEnumerable<PriceChange> changes, CancellationToken cancellationToken);
    Task<int> CountPriceChangesForDayAsync(DateOnly day, CancellationToken cancellationToken);
}
