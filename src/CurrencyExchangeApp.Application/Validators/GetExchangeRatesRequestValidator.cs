using FluentValidation;
using CurrencyExchangeApp.Core.DTOs.Requests;

namespace CurrencyExchangeApp.Application.Validators;

public class GetExchangeRatesRequestValidator : AbstractValidator<GetExchangeRatesRequest>
{
    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "NGN", "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF"
    };

    public GetExchangeRatesRequestValidator()
    {
        RuleFor(x => x.BaseCurrency)
            .NotEmpty().WithMessage("Base currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters")
            .Must(BeValidCurrency).WithMessage("Invalid currency code. Supported: NGN, USD, EUR, GBP, JPY, CAD, AUD, CHF");
    }

    private static bool BeValidCurrency(string currency)
    {
        return !string.IsNullOrEmpty(currency) && ValidCurrencies.Contains(currency.ToUpperInvariant());
    }
}
