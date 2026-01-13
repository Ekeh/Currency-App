using CurrencyExchangeApp.Core.DTOs;

namespace CurrencyExchangeApp.Web.Models.ViewModels;

public class ExchangeRatesViewModel
{
    public string BaseCurrency { get; set; } = "NGN";
    public List<ExchangeRateDto> Rates { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public List<CurrencyOption> AvailableCurrencies { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class CurrencyOption
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
