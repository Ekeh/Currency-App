namespace CurrencyExchangeApp.Core.DTOs.Requests;

public class GetCurrencyPairRequest
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
}
