namespace FuelPriceTracker.Services;

public interface ITankerkoenigApiClient
{
    Task<IReadOnlyList<StationDto>> SearchStationsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<string, StationPriceDto>> GetPricesByStationIdsAsync(IEnumerable<string> stationIds, CancellationToken cancellationToken);
}
