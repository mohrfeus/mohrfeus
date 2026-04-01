namespace FuelPriceTracker.Config;

public sealed class TankerkoenigOptions
{
    public const string SectionName = "Tankerkoenig";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://creativecommons.tankerkoenig.de/json";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 30;
    public string FuelType { get; set; } = "all";
    public string Sort { get; set; } = "dist";
}
