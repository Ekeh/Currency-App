import { useEffect, useRef, useState, type FormEvent } from "react";
import { api, ApiError } from "../api/client";
import type { Currency, CurrencyPairResponse } from "../api/types";
import { formatAmount, formatRate, formatTimestamp } from "../format";
import { CurrencySelect } from "./CurrencySelect";
import { ErrorBanner } from "./ErrorBanner";

export function Converter({ currencies, defaultFrom }: { currencies: Currency[]; defaultFrom: string }) {
  const codes = currencies.map((currency) => currency.code);
  const fallbackTo = codes.find((code) => code !== defaultFrom) ?? defaultFrom;

  const [from, setFrom] = useState(defaultFrom);
  const [to, setTo] = useState(fallbackTo);
  const [amount, setAmount] = useState("100");
  const [result, setResult] = useState<CurrencyPairResponse | null>(null);
  const [converted, setConverted] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const controller = useRef<AbortController | null>(null);

  useEffect(() => () => controller.current?.abort(), []);

  function swap() {
    setFrom(to);
    setTo(from);
    setResult(null);
    setConverted(null);
  }

  async function onSubmit(event: FormEvent) {
    event.preventDefault();
    setErrors([]);

    const value = Number(amount);
    if (!amount.trim() || !Number.isFinite(value) || value < 0) {
      setErrors(["Enter an amount of zero or more."]);
      return;
    }
    if (from === to) {
      setErrors(["Choose two different currencies."]);
      return;
    }

    controller.current?.abort();
    const next = new AbortController();
    controller.current = next;
    setLoading(true);

    try {
      const pair = await api.getPair(from, to, next.signal);
      setResult(pair);
      setConverted(value * pair.rate);
    } catch (error) {
      if (error instanceof DOMException && error.name === "AbortError") return;
      setResult(null);
      setConverted(null);
      setErrors(error instanceof ApiError ? error.messages : ["Conversion failed."]);
    } finally {
      if (controller.current === next) setLoading(false);
    }
  }

  return (
    <form onSubmit={onSubmit} noValidate aria-labelledby="converter-title">
      <h2 id="converter-title">Convert</h2>

      <div className="field">
        <label htmlFor="amount">Amount</label>
        <input
          id="amount"
          inputMode="decimal"
          value={amount}
          onChange={(event) => setAmount(event.target.value)}
        />
      </div>

      <div className="pair-row">
        <CurrencySelect id="from" label="From" value={from} currencies={currencies} onChange={setFrom} />
        <button type="button" className="btn btn-ghost swap" onClick={swap} aria-label="Swap currencies">
          ⇄
        </button>
        <CurrencySelect id="to" label="To" value={to} currencies={currencies} onChange={setTo} />
      </div>

      <ErrorBanner messages={errors} />

      <button type="submit" className="btn" disabled={loading}>
        {loading ? "Converting…" : "Convert"}
      </button>

      <div aria-live="polite" className="result">
        {result && converted !== null && (
          <>
            <p className="result-main">
              {formatAmount(Number(amount))} {result.from} = <strong>{formatAmount(converted)} {result.to}</strong>
            </p>
            <p className="muted">
              1 {result.from} = {formatRate(result.rate)} {result.to} · updated {formatTimestamp(result.lastUpdated)}
            </p>
          </>
        )}
      </div>
    </form>
  );
}
