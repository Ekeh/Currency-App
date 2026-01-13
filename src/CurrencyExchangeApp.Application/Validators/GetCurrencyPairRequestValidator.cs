using FluentValidation;
using CurrencyExchangeApp.Core.DTOs.Requests;

namespace CurrencyExchangeApp.Application.Validators;

public class GetCurrencyPairRequestValidator : AbstractValidator<GetCurrencyPairRequest>
{
    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "NGN", "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF"
    };

    public GetCurrencyPairRequestValidator()
    {
        RuleFor(x => x.FromCurrency)
            .NotEmpty().WithMessage("Source currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters")
            .Must(BeValidCurrency).WithMessage("Invalid source currency code");

        RuleFor(x => x.ToCurrency)
            .NotEmpty().WithMessage("Target currency is required")
            .Length(3).WithMessage("Currency code must be 3 characters")
            .Must(BeValidCurrency).WithMessage("Invalid target currency code");

        RuleFor(x => x)
            .Must(x => !string.Equals(x.FromCurrency, x.ToCurrency, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Source and target currencies must be different");
    }

    private static bool BeValidCurrency(string currency)
    {
        return !string.IsNullOrEmpty(currency) && ValidCurrencies.Contains(currency.ToUpperInvariant());
    }
}
