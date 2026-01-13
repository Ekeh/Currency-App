using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CurrencyExchangeApp.Core.DTOs;
using CurrencyExchangeApp.Core.DTOs.Requests;
using CurrencyExchangeApp.Core.DTOs.Responses;
using CurrencyExchangeApp.Core.Entities;
using CurrencyExchangeApp.Core.Interfaces;

namespace CurrencyExchangeApp.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExchangeRateApiClient _apiClient;
    private readonly IValidator<GetExchangeRatesRequest> _ratesValidator;
    private readonly IValidator<GetCurrencyPairRequest> _pairValidator;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly int _cacheMinutes;

    private static readonly Dictionary<string, string> SupportedCurrencies = new()
    {
        { "NGN", "Nigerian Naira" },
        { "USD", "United States Dollar" },
        { "EUR", "Euro" },
        { "GBP", "British Pound Sterling" },
        { "JPY", "Japanese Yen" },
        { "CAD", "Canadian Dollar" },
        { "AUD", "Australian Dollar" },
        { "CHF", "Swiss Franc" }
    };

    public ExchangeRateService(
        IUnitOfWork unitOfWork,
        IExchangeRateApiClient apiClient,
        IValidator<GetExchangeRatesRequest> ratesValidator,
        IValidator<GetCurrencyPairRequest> pairValidator,
        IConfiguration configuration,
        ILogger<ExchangeRateService> logger)
    {
        _unitOfWork = unitOfWork;
        _apiClient = apiClient;
        _ratesValidator = ratesValidator;
        _pairValidator = pairValidator;
        _logger = logger;
        _cacheMinutes = configuration.GetValue<int>("ExchangeRateApi:CacheMinutes", 30);
    }

    public async Task<ServiceResult<ExchangeRatesResponse>> GetExchangeRatesAsync(GetExchangeRatesRequest request)
    {
        // Validate request
        var validationResult = await _ratesValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ServiceResult<ExchangeRatesResponse>.ValidationFailure(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var baseCurrency = request.BaseCurrency.ToUpperInvariant();

        // Try to get cached rates
        var cachedRates = await _unitOfWork.CachedExchangeRates.GetValidRatesAsync(baseCurrency);
        if (cachedRates.Any())
        {
            _logger.LogInformation("Returning cached exchange rates for {BaseCurrency}", baseCurrency);
            return ServiceResult<ExchangeRatesResponse>.Success(MapToResponse(baseCurrency, cachedRates));
        }

        // Fetch from API
        var apiRates = await _apiClient.FetchRatesAsync(baseCurrency);
        if (apiRates == null)
        {
            // Use demo rates as fallback
            var demoRates = GetDemoRates(baseCurrency);
            return ServiceResult<ExchangeRatesResponse>.Success(demoRates);
        }

        // Cache and return
        var rates = await CacheAndMapRatesAsync(baseCurrency, apiRates);
        return ServiceResult<ExchangeRatesResponse>.Success(rates);
    }

    public async Task<ServiceResult<CurrencyPairResponse>> GetCurrencyPairRateAsync(GetCurrencyPairRequest request)
    {
        // Validate request
        var validationResult = await _pairValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ServiceResult<CurrencyPairResponse>.ValidationFailure(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var from = request.FromCurrency.ToUpperInvariant();
        var to = request.ToCurrency.ToUpperInvariant();

        // Get rates for the source currency
        var ratesResult = await GetExchangeRatesAsync(new GetExchangeRatesRequest { BaseCurrency = from });
        if (!ratesResult.IsSuccess)
        {
            return ServiceResult<CurrencyPairResponse>.Failure(
                ratesResult.ErrorMessage ?? "Failed to fetch exchange rates");
        }

        var rate = ratesResult.Data?.Rates.FirstOrDefault(r => r.CurrencyCode == to);
        if (rate == null)
        {
            return ServiceResult<CurrencyPairResponse>.Failure($"Rate for {from} to {to} not found");
        }

        return ServiceResult<CurrencyPairResponse>.Success(new CurrencyPairResponse
        {
            From = from,
            FromName = SupportedCurrencies[from],
            To = to,
            ToName = SupportedCurrencies[to],
            Rate = rate.Rate,
            LastUpdated = rate.LastUpdated
        });
    }

    public ServiceResult<SupportedCurrenciesResponse> GetSupportedCurrencies()
    {
        var response = new SupportedCurrenciesResponse
        {
            Currencies = SupportedCurrencies
                .Select(c => new CurrencyDto { Code = c.Key, Name = c.Value })
                .OrderBy(c => c.Code)
                .ToList(),
            Count = SupportedCurrencies.Count
        };

        return ServiceResult<SupportedCurrenciesResponse>.Success(response);
    }

    private async Task<ExchangeRatesResponse> CacheAndMapRatesAsync(
        string baseCurrency,
        Dictionary<string, decimal> apiRates)
    {
        var now = DateTime.UtcNow;
        var expiry = now.AddMinutes(_cacheMinutes);
        var rates = new List<ExchangeRateDto>();

        // Remove old cached rates
        await _unitOfWork.CachedExchangeRates.RemoveExpiredRatesAsync(baseCurrency);

        foreach (var currency in SupportedCurrencies.Keys.Where(c => c != baseCurrency))
        {
            if (apiRates.TryGetValue(currency, out var rate))
            {
                // Cache the rate
                await _unitOfWork.CachedExchangeRates.AddAsync(new CachedExchangeRate
                {
                    BaseCurrency = baseCurrency,
                    TargetCurrency = currency,
                    Rate = rate,
                    LastUpdated = now,
                    CacheExpiry = expiry
                });

                rates.Add(new ExchangeRateDto
                {
                    CurrencyCode = currency,
                    CurrencyName = SupportedCurrencies[currency],
                    Rate = rate,
                    LastUpdated = now
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Cached {Count} exchange rates for {BaseCurrency}", rates.Count, baseCurrency);

        return new ExchangeRatesResponse
        {
            BaseCurrency = baseCurrency,
            Rates = rates.OrderBy(r => r.CurrencyCode).ToList(),
            LastUpdated = now,
            Count = rates.Count
        };
    }

    private static ExchangeRatesResponse MapToResponse(
        string baseCurrency,
        IEnumerable<CachedExchangeRate> cachedRates)
    {
        var rates = cachedRates
            .Where(r => SupportedCurrencies.ContainsKey(r.TargetCurrency))
            .Select(r => new ExchangeRateDto
            {
                CurrencyCode = r.TargetCurrency,
                CurrencyName = SupportedCurrencies[r.TargetCurrency],
                Rate = r.Rate,
                LastUpdated = r.LastUpdated
            })
            .OrderBy(r => r.CurrencyCode)
            .ToList();

        return new ExchangeRatesResponse
        {
            BaseCurrency = baseCurrency,
            Rates = rates,
            LastUpdated = rates.FirstOrDefault()?.LastUpdated ?? DateTime.UtcNow,
            Count = rates.Count
        };
    }

    private static ExchangeRatesResponse GetDemoRates(string baseCurrency)
    {
        var baseRates = new Dictionary<string, decimal>
        {
            { "NGN", 1600.00m },
            { "USD", 1.00m },
            { "EUR", 0.92m },
            { "GBP", 0.79m },
            { "JPY", 154.50m },
            { "CAD", 1.36m },
            { "AUD", 1.53m },
            { "CHF", 0.88m }
        };

        var baseToUsd = baseRates[baseCurrency];
        var rates = new List<ExchangeRateDto>();

        foreach (var currency in SupportedCurrencies.Where(c => c.Key != baseCurrency))
        {
            var rate = baseRates[currency.Key] / baseToUsd;
            rates.Add(new ExchangeRateDto
            {
                CurrencyCode = currency.Key,
                CurrencyName = currency.Value,
                Rate = Math.Round(rate, 6),
                LastUpdated = DateTime.UtcNow
            });
        }

        return new ExchangeRatesResponse
        {
            BaseCurrency = baseCurrency,
            Rates = rates.OrderBy(r => r.CurrencyCode).ToList(),
            LastUpdated = DateTime.UtcNow,
            Count = rates.Count
        };
    }
}
