using CurrencyExchangeApp.Core.Interfaces;
using CurrencyExchangeApp.Infrastructure.Data;

namespace CurrencyExchangeApp.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private ICachedExchangeRateRepository? _cachedExchangeRates;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ICachedExchangeRateRepository CachedExchangeRates =>
        _cachedExchangeRates ??= new CachedExchangeRateRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
