using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CurrencyExchangeApp.Core.DTOs.Requests;
using CurrencyExchangeApp.Core.Interfaces;
using CurrencyExchangeApp.Web.Models.ViewModels;

namespace CurrencyExchangeApp.Web.Controllers;

[Authorize]
public class ExchangeRatesController : Controller
{
    private readonly IExchangeRateService _service;

    public ExchangeRatesController(IExchangeRateService service)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> Index(string? baseCurrency)
    {
        baseCurrency ??= "NGN";

        var currenciesResult = _service.GetSupportedCurrencies();
        var ratesResult = await _service.GetExchangeRatesAsync(
            new GetExchangeRatesRequest { BaseCurrency = baseCurrency });

        var viewModel = new ExchangeRatesViewModel
        {
            BaseCurrency = baseCurrency.ToUpperInvariant(),
            AvailableCurrencies = currenciesResult.Data!.Currencies
                .Select(c => new CurrencyOption { Code = c.Code, Name = c.Name })
                .ToList()
        };

        if (ratesResult.IsSuccess && ratesResult.Data != null)
        {
            viewModel.Rates = ratesResult.Data.Rates;
            viewModel.LastUpdated = ratesResult.Data.LastUpdated;
        }
        else
        {
            viewModel.ErrorMessage = ratesResult.ErrorMessage
                ?? string.Join(", ", ratesResult.ValidationErrors);
        }

        return View(viewModel);
    }
}
