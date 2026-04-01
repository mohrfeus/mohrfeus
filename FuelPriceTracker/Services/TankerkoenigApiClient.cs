using System.Net.Http.Json;
using FuelPriceTracker.Config;
using Microsoft.Extensions.Options;

namespace FuelPriceTracker.Services;

public sealed class TankerkoenigApiClient(
    HttpClient httpClient,
    IOptions<TankerkoenigOptions> options,
    ILogger<TankerkoenigApiClient> logger) : ITankerkoenigApiClient
{
    private readonly TankerkoenigOptions _options = options.Value;

    public async Task<IReadOnlyList<StationDto>> SearchStationsAsync(CancellationToken cancellationToken)
    {
        ValidateOptions();

        var query = $"stations/search?lat={_options.Latitude}&lng={_options.Longitude}&rad={_options.RadiusKm}&sort={_options.Sort}&type={_options.FuelType}&apikey={_options.ApiKey}";
        var response = await SendWithRetryAsync<StationSearchResponse>(query, cancellationToken);

        if (response is null || !response.Ok)
        {
            return [];
        }

        return response.Stations;
    }

    public async Task<IReadOnlyDictionary<string, StationPriceDto>> GetPricesByStationIdsAsync(IEnumerable<string> stationIds, CancellationToken cancellationToken)
    {
        ValidateOptions();

        var ids = string.Join(',', stationIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
        if (string.IsNullOrWhiteSpace(ids))
        {
            return new Dictionary<string, StationPriceDto>();
        }

        var query = $"stations/ids?ids={ids}&apikey={_options.ApiKey}";
        var response = await SendWithRetryAsync<StationPricesResponse>(query, cancellationToken);

        if (response is null || !response.Ok)
        {
            return new Dictionary<string, StationPriceDto>();
        }

        return response.Prices;
    }

    private async Task<T?> SendWithRetryAsync<T>(string relativePath, CancellationToken cancellationToken)
    {
        var retries = 3;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                var requestUri = new Uri(new Uri(_options.BaseUrl.TrimEnd('/') + '/'), relativePath);
                using var response = await httpClient.GetAsync(requestUri, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (attempt < retries)
            {
                logger.LogWarning(ex, "Request failed for {Path}. Attempt {Attempt}/{Retries}.", relativePath, attempt, retries);
                await Task.Delay(delay, cancellationToken);
                delay += TimeSpan.FromSeconds(2);
            }
        }

        logger.LogError("Request failed for {Path} after {Retries} attempts.", relativePath, retries);
        return default;
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Please configure Tankerkoenig:ApiKey in appsettings.json or environment variables.");
        }
    }
}
