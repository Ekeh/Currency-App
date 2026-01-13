namespace CurrencyExchangeApp.Core.Interfaces;

public interface IExchangeRateApiClient
{
    Task<Dictionary<string, decimal>?> FetchRatesAsync(string baseCurrency);
}
