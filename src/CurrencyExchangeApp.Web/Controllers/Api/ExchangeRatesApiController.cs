using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CurrencyExchangeApp.Core.DTOs.Requests;
using CurrencyExchangeApp.Core.DTOs.Responses;
using CurrencyExchangeApp.Core.Interfaces;

namespace CurrencyExchangeApp.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ExchangeRatesApiController : ControllerBase
{
    private readonly IExchangeRateService _service;

    public ExchangeRatesApiController(IExchangeRateService service)
        => _service = service;

    /// <summary>
    /// Get all exchange rates for a base currency
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ExchangeRatesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRates([FromQuery] GetExchangeRatesRequest request)
    {
        var result = await _service.GetExchangeRatesAsync(request);

        if (!result.IsSuccess)
        {
            if (result.ValidationErrors.Any())
                return BadRequest(new { Errors = result.ValidationErrors });

            return BadRequest(new { Error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get exchange rate for a specific currency pair
    /// </summary>
    [HttpGet("{from}/{to}")]
    [ProducesResponseType(typeof(CurrencyPairResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRate(string from, string to)
    {
        var request = new GetCurrencyPairRequest
        {
            FromCurrency = from,
            ToCurrency = to
        };

        var result = await _service.GetCurrencyPairRateAsync(request);

        if (!result.IsSuccess)
        {
            if (result.ValidationErrors.Any())
                return BadRequest(new { Errors = result.ValidationErrors });

            return NotFound(new { Error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of supported currencies
    /// </summary>
    [HttpGet("currencies")]
    [ProducesResponseType(typeof(SupportedCurrenciesResponse), StatusCodes.Status200OK)]
    public IActionResult GetSupportedCurrencies()
    {
        var result = _service.GetSupportedCurrencies();
        return Ok(result.Data);
    }
}
