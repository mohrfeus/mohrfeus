namespace FuelPriceTracker.Models;

public sealed class PriceSnapshot
{
    public long Id { get; set; }
    public string StationId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public decimal? Diesel { get; set; }
    public decimal? E5 { get; set; }
    public decimal? E10 { get; set; }
    public bool IsOpen { get; set; }

    public Station? Station { get; set; }
}
