import { useCallback, useEffect, useState } from "react";
import { api, ApiError } from "../api/client";
import type { Currency, ExchangeRatesResponse } from "../api/types";
import { Converter } from "../components/Converter";
import { CurrencySelect } from "../components/CurrencySelect";
import { ErrorBanner } from "../components/ErrorBanner";
import { RatesTable } from "../components/RatesTable";
import { formatTimestamp } from "../format";

const DEFAULT_BASE = "NGN";

function messagesOf(error: unknown): string[] {
  if (error instanceof ApiError) return error.messages;
  return ["Something went wrong. Please try again."];
}

export function DashboardPage() {
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [base, setBase] = useState(DEFAULT_BASE);
  const [rates, setRates] = useState<ExchangeRatesResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [errors, setErrors] = useState<string[]>([]);
  const [reloadToken, setReloadToken] = useState(0);

  // Supported currencies: loaded once.
  useEffect(() => {
    const controller = new AbortController();
    api
      .getCurrencies(controller.signal)
      .then((response) => setCurrencies(response.currencies))
      .catch((error) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) setErrors(messagesOf(error));
      });
    return () => controller.abort();
  }, []);

  // Rates: reloaded when the base currency changes or the user hits refresh.
  // The abort signal discards stale responses if the user switches quickly.
  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setErrors([]);
    api
      .getRates(base, controller.signal)
      .then((response) => {
        setRates(response);
        setLoading(false);
      })
      .catch((error) => {
        if (error instanceof DOMException && error.name === "AbortError") return;
        setRates(null);
        setErrors(messagesOf(error));
        setLoading(false);
      });
    return () => controller.abort();
  }, [base, reloadToken]);

  const refresh = useCallback(() => setReloadToken((value) => value + 1), []);

  return (
    <div className="grid">
      <section className="card" aria-labelledby="rates-title">
        <div className="card-head">
          <h2 id="rates-title">Exchange rates</h2>
          <button type="button" className="btn btn-ghost" onClick={refresh} disabled={loading}>
            Refresh
          </button>
        </div>

        {currencies.length > 0 && (
          <CurrencySelect id="base" label="Base currency" value={base} currencies={currencies} onChange={setBase} />
        )}

        <ErrorBanner messages={errors} />

        {loading && (
          <p role="status" className="muted">
            Loading rates…
          </p>
        )}

        {!loading && rates && (
          <>
            <p className="muted">
              {rates.count} currencies · updated {formatTimestamp(rates.lastUpdated)}
            </p>
            <RatesTable baseCurrency={rates.baseCurrency} rates={rates.rates} />
          </>
        )}
      </section>

      <section className="card">
        {currencies.length > 1 ? (
          <Converter currencies={currencies} defaultFrom={DEFAULT_BASE} />
        ) : (
          <p className="muted">Converter will appear when currencies have loaded.</p>
        )}
      </section>
    </div>
  );
}
