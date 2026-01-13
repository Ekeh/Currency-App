using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CurrencyExchangeApp.Core.Interfaces;

namespace CurrencyExchangeApp.Infrastructure.External;

public class ExchangeRateApiClient : IExchangeRateApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExchangeRateApiClient> _logger;

    public ExchangeRateApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ExchangeRateApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Dictionary<string, decimal>?> FetchRatesAsync(string baseCurrency)
    {
        var apiKey = _configuration["ExchangeRateApi:ApiKey"];
        var baseUrl = _configuration["ExchangeRateApi:BaseUrl"] ?? "https://v6.exchangerate-api.com/v6";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            _logger.LogWarning("API key not configured");
            return null;
        }

        try
        {
            var requestUrl = $"{baseUrl}/{apiKey}/latest/{baseCurrency}";
            _logger.LogInformation("Fetching exchange rates from API for {BaseCurrency}", baseCurrency);

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ExchangeRateApiResponse>(content);

            if (apiResponse?.Result != "success" || apiResponse.ConversionRates == null)
            {
                _logger.LogError("API returned unsuccessful result");
                return null;
            }

            return apiResponse.ConversionRates;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rates from API");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse API response");
            return null;
        }
    }
}

internal class ExchangeRateApiResponse
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal>? ConversionRates { get; set; }
}
