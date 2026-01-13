using CurrencyExchangeApp.Core.DTOs;
using CurrencyExchangeApp.Core.DTOs.Requests;
using CurrencyExchangeApp.Core.DTOs.Responses;

namespace CurrencyExchangeApp.Core.Interfaces;

public interface IExchangeRateService
{
    Task<ServiceResult<ExchangeRatesResponse>> GetExchangeRatesAsync(GetExchangeRatesRequest request);
    Task<ServiceResult<CurrencyPairResponse>> GetCurrencyPairRateAsync(GetCurrencyPairRequest request);
    ServiceResult<SupportedCurrenciesResponse> GetSupportedCurrencies();
}
