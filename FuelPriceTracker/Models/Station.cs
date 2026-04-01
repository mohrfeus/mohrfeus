namespace FuelPriceTracker.Models;

public sealed class Station
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }

    public ICollection<PriceSnapshot> PriceSnapshots { get; set; } = new List<PriceSnapshot>();
}
