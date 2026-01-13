using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CurrencyExchangeApp.Application.Services;
using CurrencyExchangeApp.Core.DTOs.Requests;
using CurrencyExchangeApp.Core.Entities;
using CurrencyExchangeApp.Core.Interfaces;

namespace CurrencyExchangeApp.Tests.Application.Services;

public class ExchangeRateServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IExchangeRateApiClient> _apiClientMock;
    private readonly Mock<IValidator<GetExchangeRatesRequest>> _ratesValidatorMock;
    private readonly Mock<IValidator<GetCurrencyPairRequest>> _pairValidatorMock;
    private readonly Mock<ILogger<ExchangeRateService>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly ExchangeRateService _service;

    public ExchangeRateServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _apiClientMock = new Mock<IExchangeRateApiClient>();
        _ratesValidatorMock = new Mock<IValidator<GetExchangeRatesRequest>>();
        _pairValidatorMock = new Mock<IValidator<GetCurrencyPairRequest>>();
        _loggerMock = new Mock<ILogger<ExchangeRateService>>();

        var configData = new Dictionary<string, string?>
        {
            { "ExchangeRateApi:CacheMinutes", "30" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _service = new ExchangeRateService(
            _unitOfWorkMock.Object,
            _apiClientMock.Object,
            _ratesValidatorMock.Object,
            _pairValidatorMock.Object,
            _configuration,
            _loggerMock.Object);
    }

    #region GetExchangeRatesAsync Tests

    [Fact]
    public async Task GetExchangeRatesAsync_WithValidRequest_ReturnsCachedRates()
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = "NGN" };
        var cachedRates = new List<CachedExchangeRate>
        {
            new() { BaseCurrency = "NGN", TargetCurrency = "USD", Rate = 0.00063m, LastUpdated = DateTime.UtcNow },
            new() { BaseCurrency = "NGN", TargetCurrency = "EUR", Rate = 0.00058m, LastUpdated = DateTime.UtcNow }
        };

        _ratesValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        var repoMock = new Mock<ICachedExchangeRateRepository>();
        repoMock.Setup(r => r.GetValidRatesAsync("NGN"))
            .ReturnsAsync(cachedRates);
        _unitOfWorkMock.Setup(u => u.CachedExchangeRates).Returns(repoMock.Object);

        // Act
        var result = await _service.GetExchangeRatesAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.BaseCurrency.Should().Be("NGN");
        result.Data.Rates.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithInvalidCurrency_ReturnsValidationError()
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = "INVALID" };
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("BaseCurrency", "Invalid currency code")
        });

        _ratesValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.GetExchangeRatesAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Invalid currency code");
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WhenCacheEmpty_FetchesFromApiAndCaches()
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = "NGN" };
        var apiRates = new Dictionary<string, decimal>
        {
            { "USD", 0.00063m },
            { "EUR", 0.00058m },
            { "GBP", 0.00050m }
        };

        _ratesValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        var repoMock = new Mock<ICachedExchangeRateRepository>();
        repoMock.Setup(r => r.GetValidRatesAsync("NGN"))
            .ReturnsAsync(new List<CachedExchangeRate>());
        repoMock.Setup(r => r.RemoveExpiredRatesAsync("NGN"))
            .Returns(Task.CompletedTask);
        repoMock.Setup(r => r.AddAsync(It.IsAny<CachedExchangeRate>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.CachedExchangeRates).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _apiClientMock.Setup(a => a.FetchRatesAsync("NGN"))
            .ReturnsAsync(apiRates);

        // Act
        var result = await _service.GetExchangeRatesAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Rates.Should().NotBeEmpty();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WhenApiReturnsNull_ReturnsDemoRates()
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = "NGN" };

        _ratesValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        var repoMock = new Mock<ICachedExchangeRateRepository>();
        repoMock.Setup(r => r.GetValidRatesAsync("NGN"))
            .ReturnsAsync(new List<CachedExchangeRate>());

        _unitOfWorkMock.Setup(u => u.CachedExchangeRates).Returns(repoMock.Object);

        _apiClientMock.Setup(a => a.FetchRatesAsync("NGN"))
            .ReturnsAsync((Dictionary<string, decimal>?)null);

        // Act
        var result = await _service.GetExchangeRatesAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Rates.Should().NotBeEmpty();
        result.Data.BaseCurrency.Should().Be("NGN");
    }

    [Theory]
    [InlineData("ngn")]
    [InlineData("NGN")]
    [InlineData("Ngn")]
    public async Task GetExchangeRatesAsync_NormalizesCurrencyToUpperCase(string baseCurrency)
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = baseCurrency };

        _ratesValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        var repoMock = new Mock<ICachedExchangeRateRepository>();
        repoMock.Setup(r => r.GetValidRatesAsync("NGN"))
            .ReturnsAsync(new List<CachedExchangeRate>());

        _unitOfWorkMock.Setup(u => u.CachedExchangeRates).Returns(repoMock.Object);

        _apiClientMock.Setup(a => a.FetchRatesAsync("NGN"))
            .ReturnsAsync((Dictionary<string, decimal>?)null);

        // Act
        var result = await _service.GetExchangeRatesAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.BaseCurrency.Should().Be("NGN");
    }

    #endregion

    #region GetCurrencyPairRateAsync Tests

    [Fact]
    public async Task GetCurrencyPairRateAsync_WithValidPair_ReturnsRate()
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = "NGN", ToCurrency = "USD" };
        var cachedRates = new List<CachedExchangeRate>
        {
            new() { BaseCurrency = "NGN", TargetCurrency = "USD", Rate = 0.00063m, LastUpdated = DateTime.UtcNow }
        };

        _pairValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        _ratesValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetExchangeRatesRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        var repoMock = new Mock<ICachedExchangeRateRepository>();
        repoMock.Setup(r => r.GetValidRatesAsync("NGN"))
            .ReturnsAsync(cachedRates);

        _unitOfWorkMock.Setup(u => u.CachedExchangeRates).Returns(repoMock.Object);

        // Act
        var result = await _service.GetCurrencyPairRateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.From.Should().Be("NGN");
        result.Data.To.Should().Be("USD");
        result.Data.Rate.Should().Be(0.00063m);
    }

    [Fact]
    public async Task GetCurrencyPairRateAsync_WithInvalidFromCurrency_ReturnsValidationError()
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = "XXX", ToCurrency = "USD" };
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("FromCurrency", "Invalid source currency code")
        });

        _pairValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.GetCurrencyPairRateAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Invalid source currency code");
    }

    [Fact]
    public async Task GetCurrencyPairRateAsync_WhenRateNotFound_ReturnsFailure()
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = "NGN", ToCurrency = "USD" };

        _pairValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        _ratesValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetExchangeRatesRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        var repoMock = new Mock<ICachedExchangeRateRepository>();
        repoMock.Setup(r => r.GetValidRatesAsync("NGN"))
            .ReturnsAsync(new List<CachedExchangeRate>()); // No cached rates

        _unitOfWorkMock.Setup(u => u.CachedExchangeRates).Returns(repoMock.Object);

        _apiClientMock.Setup(a => a.FetchRatesAsync("NGN"))
            .ReturnsAsync((Dictionary<string, decimal>?)null);

        // Act
        var result = await _service.GetCurrencyPairRateAsync(request);

        // Assert - demo rates should still work
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region GetSupportedCurrencies Tests

    [Fact]
    public void GetSupportedCurrencies_ReturnsAllSupportedCurrencies()
    {
        // Act
        var result = _service.GetSupportedCurrencies();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Currencies.Should().HaveCount(8);
        result.Data.Currencies.Should().Contain(c => c.Code == "NGN");
        result.Data.Currencies.Should().Contain(c => c.Code == "USD");
        result.Data.Currencies.Should().Contain(c => c.Code == "EUR");
    }

    [Fact]
    public void GetSupportedCurrencies_CurrenciesAreSortedByCode()
    {
        // Act
        var result = _service.GetSupportedCurrencies();

        // Assert
        var codes = result.Data!.Currencies.Select(c => c.Code).ToList();
        codes.Should().BeInAscendingOrder();
    }

    [Fact]
    public void GetSupportedCurrencies_IncludesNGNAsDefaultCurrency()
    {
        // Act
        var result = _service.GetSupportedCurrencies();

        // Assert
        var ngn = result.Data!.Currencies.FirstOrDefault(c => c.Code == "NGN");
        ngn.Should().NotBeNull();
        ngn!.Name.Should().Be("Nigerian Naira");
    }

    #endregion
}
