using CurrencyExchangeApp.Application.Validators;
using CurrencyExchangeApp.Core.DTOs.Requests;

namespace CurrencyExchangeApp.Tests.Application.Validators;

public class GetCurrencyPairRequestValidatorTests
{
    private readonly GetCurrencyPairRequestValidator _validator;

    public GetCurrencyPairRequestValidatorTests()
    {
        _validator = new GetCurrencyPairRequestValidator();
    }

    [Theory]
    [InlineData("NGN", "USD")]
    [InlineData("USD", "EUR")]
    [InlineData("GBP", "JPY")]
    [InlineData("CAD", "AUD")]
    public async Task Validate_WithValidCurrencyPair_ShouldPass(string from, string to)
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = from, ToCurrency = to };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("ngn", "usd")]
    [InlineData("Ngn", "Usd")]
    public async Task Validate_WithLowercaseCurrencies_ShouldPass(string from, string to)
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = from, ToCurrency = to };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyFromCurrency_ShouldFail()
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = "", ToCurrency = "USD" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Source currency is required");
    }

    [Fact]
    public async Task Validate_WithEmptyToCurrency_ShouldFail()
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = "NGN", ToCurrency = "" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Target currency is required");
    }

    [Theory]
    [InlineData("US", "USD")]
    [InlineData("NGN", "US")]
    [InlineData("AB", "CD")]
    public async Task Validate_WithInvalidLength_ShouldFail(string from, string to)
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = from, ToCurrency = to };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Currency code must be 3 characters");
    }

    [Theory]
    [InlineData("XXX", "USD")]
    [InlineData("NGN", "XXX")]
    public async Task Validate_WithUnsupportedCurrency_ShouldFail(string from, string to)
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = from, ToCurrency = to };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Invalid"));
    }

    [Theory]
    [InlineData("USD", "USD")]
    [InlineData("NGN", "NGN")]
    [InlineData("usd", "USD")]
    public async Task Validate_WithSameCurrencies_ShouldFail(string from, string to)
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = from, ToCurrency = to };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Source and target currencies must be different");
    }

    [Fact]
    public async Task Validate_WithBothCurrenciesNull_ShouldHaveMultipleErrors()
    {
        // Arrange
        var request = new GetCurrencyPairRequest { FromCurrency = null!, ToCurrency = null! };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }
}
