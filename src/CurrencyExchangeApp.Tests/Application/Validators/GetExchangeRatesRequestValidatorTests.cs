using CurrencyExchangeApp.Application.Validators;
using CurrencyExchangeApp.Core.DTOs.Requests;

namespace CurrencyExchangeApp.Tests.Application.Validators;

public class GetExchangeRatesRequestValidatorTests
{
    private readonly GetExchangeRatesRequestValidator _validator;

    public GetExchangeRatesRequestValidatorTests()
    {
        _validator = new GetExchangeRatesRequestValidator();
    }

    [Theory]
    [InlineData("NGN")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("CAD")]
    [InlineData("AUD")]
    [InlineData("CHF")]
    public async Task Validate_WithValidCurrency_ShouldPass(string currency)
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = currency };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("ngn")]
    [InlineData("usd")]
    [InlineData("Eur")]
    public async Task Validate_WithLowercaseCurrency_ShouldPass(string currency)
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = currency };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyCurrency_ShouldFail()
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = "" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Base currency is required");
    }

    [Fact]
    public async Task Validate_WithNullCurrency_ShouldFail()
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = null! };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Base currency is required");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("A")]
    public async Task Validate_WithInvalidLength_ShouldFail(string currency)
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = currency };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Currency code must be 3 characters");
    }

    [Theory]
    [InlineData("XXX")]
    [InlineData("ABC")]
    [InlineData("ZZZ")]
    public async Task Validate_WithUnsupportedCurrency_ShouldFail(string currency)
    {
        // Arrange
        var request = new GetExchangeRatesRequest { BaseCurrency = currency };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Invalid currency code"));
    }
}
