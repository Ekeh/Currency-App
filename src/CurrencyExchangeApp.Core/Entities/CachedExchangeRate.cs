namespace CurrencyExchangeApp.Core.Entities;

public class CachedExchangeRate : BaseEntity
{
    public string BaseCurrency { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime CacheExpiry { get; set; }
}
