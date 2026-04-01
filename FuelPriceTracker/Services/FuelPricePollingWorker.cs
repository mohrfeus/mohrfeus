using FuelPriceTracker.Config;
using FuelPriceTracker.Models;
using FuelPriceTracker.Repositories;
using Microsoft.Extensions.Options;

namespace FuelPriceTracker.Services;

public sealed class FuelPricePollingWorker(
    ITankerkoenigApiClient apiClient,
    IStationRepository stationRepository,
    IPriceSnapshotRepository snapshotRepository,
    IOptions<PollingOptions> pollingOptions,
    ILogger<FuelPricePollingWorker> logger) : BackgroundService
{
    private readonly PollingOptions _pollingOptions = pollingOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Fuel price polling worker started.");

        await InitialStationSyncAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1, _pollingOptions.IntervalMinutes)));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PollPricesAsync(stoppingToken);
        }
    }

    private async Task InitialStationSyncAsync(CancellationToken cancellationToken)
    {
        var stations = await apiClient.SearchStationsAsync(cancellationToken);

        var mapped = stations.Select(x => new Station
        {
            Id = x.Id,
            Name = x.Name,
            Brand = x.Brand,
            Street = x.Street,
            Place = x.Place,
            Lat = x.Lat,
            Lng = x.Lng
        }).ToList();

        await stationRepository.UpsertStationsAsync(mapped, cancellationToken);
        logger.LogInformation("Station synchronization complete. Total stations tracked: {Count}", mapped.Count);

        await PollPricesAsync(cancellationToken);
    }

    private async Task PollPricesAsync(CancellationToken cancellationToken)
    {
        var timestampUtc = DateTime.UtcNow;
        var stations = await stationRepository.GetAllAsync(cancellationToken);

        if (stations.Count == 0)
        {
            logger.LogWarning("No stations available for polling.");
            return;
        }

        var stationIds = stations.Select(x => x.Id).ToList();
        var latestSnapshots = await snapshotRepository.GetLatestPerStationAsync(stationIds, cancellationToken);
        var currentPrices = await apiClient.GetPricesByStationIdsAsync(stationIds, cancellationToken);

        var snapshotsToInsert = new List<PriceSnapshot>();
        var changesToInsert = new List<PriceChange>();

        foreach (var stationId in stationIds)
        {
            if (!currentPrices.TryGetValue(stationId, out var price))
            {
                continue;
            }

            var isOpen = price.Status.Equals("open", StringComparison.OrdinalIgnoreCase);

            var snapshot = new PriceSnapshot
            {
                StationId = stationId,
                TimestampUtc = timestampUtc,
                Diesel = isOpen ? price.Diesel : null,
                E5 = isOpen ? price.E5 : null,
                E10 = isOpen ? price.E10 : null,
                IsOpen = isOpen
            };

            latestSnapshots.TryGetValue(stationId, out var previous);

            var hasChanged = previous is null
                             || previous.IsOpen != snapshot.IsOpen
                             || previous.Diesel != snapshot.Diesel
                             || previous.E5 != snapshot.E5
                             || previous.E10 != snapshot.E10;

            if (!hasChanged && _pollingOptions.SkipUnchangedSnapshots)
            {
                continue;
            }

            snapshotsToInsert.Add(snapshot);

            if (previous is not null && hasChanged)
            {
                changesToInsert.Add(new PriceChange
                {
                    StationId = stationId,
                    TimestampUtc = timestampUtc,
                    PreviousDiesel = previous.Diesel,
                    NewDiesel = snapshot.Diesel,
                    PreviousE5 = previous.E5,
                    NewE5 = snapshot.E5,
                    PreviousE10 = previous.E10,
                    NewE10 = snapshot.E10
                });
            }
        }

        await snapshotRepository.SaveSnapshotsAsync(snapshotsToInsert, changesToInsert, cancellationToken);

        var changeCountToday = await snapshotRepository.CountPriceChangesForDayAsync(DateOnly.FromDateTime(timestampUtc), cancellationToken);

        logger.LogInformation(
            "Polling completed at {TimestampUtc}. New snapshots: {SnapshotCount}, New price changes: {ChangeCount}, Price changes today: {DailyChangeCount}.",
            timestampUtc,
            snapshotsToInsert.Count,
            changesToInsert.Count,
            changeCountToday);
    }
}
