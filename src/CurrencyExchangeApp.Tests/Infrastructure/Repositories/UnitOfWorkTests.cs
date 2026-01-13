using Microsoft.EntityFrameworkCore;
using CurrencyExchangeApp.Core.Entities;
using CurrencyExchangeApp.Infrastructure.Data;
using CurrencyExchangeApp.Infrastructure.Repositories;

namespace CurrencyExchangeApp.Tests.Infrastructure.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    [Fact]
    public void CachedExchangeRates_ReturnsSameInstance()
    {
        // Act
        var first = _unitOfWork.CachedExchangeRates;
        var second = _unitOfWork.CachedExchangeRates;

        // Assert
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void CachedExchangeRates_ReturnsValidRepository()
    {
        // Act
        var repo = _unitOfWork.CachedExchangeRates;

        // Assert
        repo.Should().NotBeNull();
        repo.Should().BeAssignableTo<CachedExchangeRateRepository>();
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsChanges()
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
        await _unitOfWork.CachedExchangeRates.AddAsync(rate);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        var saved = await _context.CachedExchangeRates.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoChanges_ReturnsZero()
    {
        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleChanges_ReturnsCorrectCount()
    {
        // Arrange
        var rates = new List<CachedExchangeRate>
        {
            new() { BaseCurrency = "NGN", TargetCurrency = "USD", Rate = 0.00063m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) },
            new() { BaseCurrency = "NGN", TargetCurrency = "EUR", Rate = 0.00058m, LastUpdated = DateTime.UtcNow, CacheExpiry = DateTime.UtcNow.AddMinutes(30) }
        };
        await _unitOfWork.CachedExchangeRates.AddRangeAsync(rates);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Act & Assert - Should not throw
        var action = () =>
        {
            _unitOfWork.Dispose();
            _unitOfWork.Dispose();
        };

        action.Should().NotThrow();
    }
}
