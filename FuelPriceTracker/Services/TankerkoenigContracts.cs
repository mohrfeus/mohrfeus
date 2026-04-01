using System.Text.Json.Serialization;

namespace FuelPriceTracker.Services;

public sealed class StationSearchResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("stations")]
    public List<StationDto> Stations { get; set; } = [];
}

public sealed class StationDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("brand")]
    public string Brand { get; set; } = string.Empty;

    [JsonPropertyName("street")]
    public string Street { get; set; } = string.Empty;

    [JsonPropertyName("place")]
    public string Place { get; set; } = string.Empty;

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}

public sealed class StationPricesResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("prices")]
    public Dictionary<string, StationPriceDto> Prices { get; set; } = [];
}

public sealed class StationPriceDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("diesel")]
    public decimal? Diesel { get; set; }

    [JsonPropertyName("e5")]
    public decimal? E5 { get; set; }

    [JsonPropertyName("e10")]
    public decimal? E10 { get; set; }
}
