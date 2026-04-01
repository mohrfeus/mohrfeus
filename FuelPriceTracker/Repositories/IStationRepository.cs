using FuelPriceTracker.Models;

namespace FuelPriceTracker.Repositories;

public interface IStationRepository
{
    Task UpsertStationsAsync(IEnumerable<Station> stations, CancellationToken cancellationToken);
    Task<List<Station>> GetAllAsync(CancellationToken cancellationToken);
}
