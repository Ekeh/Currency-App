using CurrencyExchangeApp.Core.Entities;

namespace CurrencyExchangeApp.Core.Interfaces;

public interface ICachedExchangeRateRepository : IRepository<CachedExchangeRate>
{
    Task<IEnumerable<CachedExchangeRate>> GetValidRatesAsync(string baseCurrency);
    Task RemoveExpiredRatesAsync(string baseCurrency);
}
