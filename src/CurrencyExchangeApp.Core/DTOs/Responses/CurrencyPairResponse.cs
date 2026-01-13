namespace CurrencyExchangeApp.Core.DTOs.Responses;

public class CurrencyPairResponse
{
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime LastUpdated { get; set; }
}
