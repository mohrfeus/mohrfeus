using FuelPriceTracker.Data;
using FuelPriceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FuelPriceTracker.Repositories;

public sealed class StationRepository(FuelPriceDbContext dbContext) : IStationRepository
{
    public async Task UpsertStationsAsync(IEnumerable<Station> stations, CancellationToken cancellationToken)
    {
        var incoming = stations.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var existing = await dbContext.Stations
            .Where(x => incoming.Keys.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var station in incoming.Values)
        {
            if (existing.TryGetValue(station.Id, out var current))
            {
                current.Name = station.Name;
                current.Brand = station.Brand;
                current.Street = station.Street;
                current.Place = station.Place;
                current.Lat = station.Lat;
                current.Lng = station.Lng;
            }
            else
            {
                await dbContext.Stations.AddAsync(station, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<Station>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.Stations.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
    }
}
