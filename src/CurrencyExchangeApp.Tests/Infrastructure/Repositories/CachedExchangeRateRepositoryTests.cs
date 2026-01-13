using Microsoft.EntityFrameworkCore;
using CurrencyExchangeApp.Core.Entities;
using CurrencyExchangeApp.Infrastructure.Data;
using CurrencyExchangeApp.Infrastructure.Repositories;

namespace CurrencyExchangeApp.Tests.Infrastructure.Repositories;

public class CachedExchangeRateRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CachedExchangeRateRepository _repository;

    public CachedExchangeRateRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CachedExchangeRateRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetValidRatesAsync Tests

    [Fact]
    public async Task GetValidRatesAsync_WithValidRates_ReturnsRates()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new()
            {
                BaseCurrency = "NGN",
                TargetCurrency = "USD",
                Rate = 0.00063m,
                LastUpdated = DateTime.UtcNow,
                CacheExpiry = DateTime.UtcNow.AddMinutes(30)
            },
            new()
            {
                BaseCurrency = "NGN",
                TargetCurrency = "EUR",
                Rate = 0.00058m,
                LastUpdated = DateTime.UtcNow,
                CacheExpiry = DateTime.UtcNow.AddMinutes(30)
            }
        };
        await _context.CachedExchangeRates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidRatesAsync("NGN");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.BaseCurrency.Should().Be("NGN"));
    }

    [Fact]
    public async Task GetValidRatesAsync_WithExpiredRates_ReturnsEmpty()
    {
        // Arrange
        var rate = new CachedExchangeRate
        {
            BaseCurrency = "NGN",
            TargetCurrency = "USD",
            Rate = 0.00063m,
            LastUpdated = DateTime.UtcNow.AddHours(-1),
            CacheExpiry = DateTime.UtcNow.AddMinutes(-30) // Expired
        };
        await _context.CachedExchangeRates.AddAsync(rate);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidRatesAsync("NGN");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetValidRatesAsync_WithDifferentBaseCurrency_ReturnsEmpty()
    {
        // Arrange
        var rate = new CachedExchangeRate
        {
            BaseCurrency = "USD",
            TargetCurrency = "EUR",
            Rate = 0.92m,
            LastUpdated = DateTime.UtcNow,
            CacheExpiry = DateTime.UtcNow.AddMinutes(30)
        };
        await _context.CachedExchangeRates.AddAsync(rate);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidRatesAsync("NGN");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetValidRatesAsync_WithMixedRates_ReturnsOnlyValid()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new()
            {
                BaseCurrency = "NGN",
                TargetCurrency = "USD",
                Rate = 0.00063m,
                LastUpdated = DateTime.UtcNow,
                CacheExpiry = DateTime.UtcNow.AddMinutes(30) // Valid
            },
            new()
            {
                BaseCurrency = "NGN",
                TargetCurrency = "EUR",
                Rate = 0.00058m,
                LastUpdated = DateTime.UtcNow.AddHours(-1),
                CacheExpiry = DateTime.UtcNow.AddMinutes(-30) // Expired
            }
        };
        await _context.CachedExchangeRates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidRatesAsync("NGN");

        // Assert
        result.Should().HaveCount(1);
        result.First().TargetCurrency.Should().Be("USD");
    }

    #endregion

    #region RemoveExpiredRatesAsync Tests

    [Fact]
    public async Task RemoveExpiredRatesAsync_RemovesAllRatesForCurrency()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new()
            {
                BaseCurrency = "NGN",
                TargetCurrency = "USD",
                Rate = 0.00063m,
                LastUpdated = DateTime.UtcNow,
                CacheExpiry = DateTime.UtcNow.AddMinutes(30)
            },
            new()
            {
                BaseCurrency = "NGN",
                TargetCurrency = "EUR",
                Rate = 0.00058m,
                LastUpdated = DateTime.UtcNow,
                CacheExpiry = DateTime.UtcNow.AddMinutes(30)
            }
        };
        await _context.CachedExchangeRates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveExpiredRatesAsync("NGN");
        await _context.SaveChangesAsync();

        // Assert
        var remaining = await _context.CachedExchangeRates.Where(r => r.BaseCurrency == "NGN").ToListAsync();
        remaining.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveExpiredRatesAsync_DoesNotAffectOtherCurrencies()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new()
            {
                BaseCurrency = "NGN",
                TargetCurrency = "USD",
                Rate = 0.00063m,
                LastUpdated = DateTime.UtcNow,
                CacheExpiry = DateTime.UtcNow.AddMinutes(30)
            },
            new()
            {
                BaseCurrency = "USD",
                TargetCurrency = "EUR",
                Rate = 0.92m,
                LastUpdated = DateTime.UtcNow,
                CacheExpiry = DateTime.UtcNow.AddMinutes(30)
            }
        };
        await _context.CachedExchangeRates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveExpiredRatesAsync("NGN");
        await _context.SaveChangesAsync();

        // Assert
        var remaining = await _context.CachedExchangeRates.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining.First().BaseCurrency.Should().Be("USD");
    }

    #endregion

    #region Base Repository Tests

    [Fact]
    public async Task AddAsync_AddsEntityToDatabase()
    {
        // Arrange
        var rate = new CachedExchangeRate
        {
            BaseCurrency = "NGN",
            TargetCurrency = "USD",
            Rate = 0.00063m,
            LastUpdated = DateTime.UtcNow,
            CacheExpiry = DateTime.UtcNow.AddMinutes(30)
        };

        // Act
        await _repository.AddAsync(rate);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.CachedExchangeRates.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.BaseCurrency.Should().Be("NGN");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity()
    {
        // Arrange
        var rate = new CachedExchangeRate
        {
            BaseCurrency = "NGN",
            TargetCurrency = "USD",
            Rate = 0.00063m,
            LastUpdated = DateTime.UtcNow,
            CacheExpiry = DateTime.UtcNow.AddMinutes(30)
        };
        await _context.CachedExchangeRates.AddAsync(rate);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(rate.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(rate.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new() { BaseCurrency = "NGN", TargetCurrency = "USD", Rate = 0.00063m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) },
            new() { BaseCurrency = "NGN", TargetCurrency = "EUR", Rate = 0.00058m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) }
        };
        await _context.CachedExchangeRates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ReturnsMatchingEntities()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new() { BaseCurrency = "NGN", TargetCurrency = "USD", Rate = 0.00063m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) },
            new() { BaseCurrency = "USD", TargetCurrency = "EUR", Rate = 0.92m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) }
        };
        await _context.CachedExchangeRates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(r => r.BaseCurrency == "NGN");

        // Assert
        result.Should().HaveCount(1);
        result.First().BaseCurrency.Should().Be("NGN");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ReturnsFirstMatch()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new() { BaseCurrency = "NGN", TargetCurrency = "USD", Rate = 0.00063m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) },
            new() { BaseCurrency = "NGN", TargetCurrency = "EUR", Rate = 0.00058m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) }
        };
        await _context.CachedExchangeRates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FirstOrDefaultAsync(r => r.BaseCurrency == "NGN");

        // Assert
        result.Should().NotBeNull();
        result!.BaseCurrency.Should().Be("NGN");
    }

    [Fact]
    public void Update_ModifiesEntity()
    {
        // Arrange
        var rate = new CachedExchangeRate
        {
            BaseCurrency = "NGN",
            TargetCurrency = "USD",
            Rate = 0.00063m,
            LastUpdated = DateTime.UtcNow,
            CacheExpiry = DateTime.UtcNow.AddMinutes(30)
        };
        _context.CachedExchangeRates.Add(rate);
        _context.SaveChanges();

        // Act
        rate.Rate = 0.00065m;
        _repository.Update(rate);
        _context.SaveChanges();

        // Assert
        var updated = _context.CachedExchangeRates.Find(rate.Id);
        updated!.Rate.Should().Be(0.00065m);
    }

    [Fact]
    public void Remove_DeletesEntity()
    {
        // Arrange
        var rate = new CachedExchangeRate
        {
            BaseCurrency = "NGN",
            TargetCurrency = "USD",
            Rate = 0.00063m,
            LastUpdated = DateTime.UtcNow,
            CacheExpiry = DateTime.UtcNow.AddMinutes(30)
        };
        _context.CachedExchangeRates.Add(rate);
        _context.SaveChanges();

        // Act
        _repository.Remove(rate);
        _context.SaveChanges();

        // Assert
        var deleted = _context.CachedExchangeRates.Find(rate.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task AddRangeAsync_AddsMultipleEntities()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new() { BaseCurrency = "NGN", TargetCurrency = "USD", Rate = 0.00063m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) },
            new() { BaseCurrency = "NGN", TargetCurrency = "EUR", Rate = 0.00058m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) }
        };

        // Act
        await _repository.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Assert
        var count = await _context.CachedExchangeRates.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public void RemoveRange_DeletesMultipleEntities()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new() { BaseCurrency = "NGN", TargetCurrency = "USD", Rate = 0.00063m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) },
            new() { BaseCurrency = "NGN", TargetCurrency = "EUR", Rate = 0.00058m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) }
        };
        _context.CachedExchangeRates.AddRange(rates);
        _context.SaveChanges();

        // Act
        _repository.RemoveRange(rates);
        _context.SaveChanges();

        // Assert
        var count = _context.CachedExchangeRates.Count();
        count.Should().Be(0);
    }

    #endregion
}
