namespace CurrencyExchangeApp.Core.DTOs.Responses;

public class ExchangeRatesResponse
{
    public string BaseCurrency { get; set; } = string.Empty;
    public List<ExchangeRateDto> Rates { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public int Count { get; set; }
}
