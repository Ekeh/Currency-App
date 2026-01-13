using Microsoft.EntityFrameworkCore;
using CurrencyExchangeApp.Core.Entities;
using CurrencyExchangeApp.Core.Interfaces;
using CurrencyExchangeApp.Infrastructure.Data;

namespace CurrencyExchangeApp.Infrastructure.Repositories;

public class CachedExchangeRateRepository : Repository<CachedExchangeRate>, ICachedExchangeRateRepository
{
    public CachedExchangeRateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CachedExchangeRate>> GetValidRatesAsync(string baseCurrency)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(r => r.BaseCurrency == baseCurrency && r.CacheExpiry > now)
            .ToListAsync();
    }

    public async Task RemoveExpiredRatesAsync(string baseCurrency)
    {
        var expiredRates = await _dbSet
            .Where(r => r.BaseCurrency == baseCurrency)
            .ToListAsync();

        _dbSet.RemoveRange(expiredRates);
    }
}
