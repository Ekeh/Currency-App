namespace CurrencyExchangeApp.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICachedExchangeRateRepository CachedExchangeRates { get; }
    Task<int> SaveChangesAsync();
}
