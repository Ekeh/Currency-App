namespace CurrencyExchangeApp.Core.DTOs.Responses;

public class SupportedCurrenciesResponse
{
    public List<CurrencyDto> Currencies { get; set; } = new();
    public int Count { get; set; }
}

public class CurrencyDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
