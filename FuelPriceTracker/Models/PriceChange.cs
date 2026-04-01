namespace FuelPriceTracker.Models;

public sealed class PriceChange
{
    public long Id { get; set; }
    public string StationId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public decimal? PreviousDiesel { get; set; }
    public decimal? NewDiesel { get; set; }
    public decimal? PreviousE5 { get; set; }
    public decimal? NewE5 { get; set; }
    public decimal? PreviousE10 { get; set; }
    public decimal? NewE10 { get; set; }

    public Station? Station { get; set; }
}
