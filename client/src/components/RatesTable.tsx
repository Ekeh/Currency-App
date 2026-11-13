import { useMemo, useState } from "react";
import type { ExchangeRate } from "../api/types";
import { formatRate } from "../format";

type SortKey = "currencyCode" | "rate";

export function RatesTable({ baseCurrency, rates }: { baseCurrency: string; rates: ExchangeRate[] }) {
  const [filter, setFilter] = useState("");
  const [sortKey, setSortKey] = useState<SortKey>("currencyCode");
  const [ascending, setAscending] = useState(true);

  const visible = useMemo(() => {
    const needle = filter.trim().toLowerCase();
    const filtered = needle
      ? rates.filter(
          (rate) =>
            rate.currencyCode.toLowerCase().includes(needle) || rate.currencyName.toLowerCase().includes(needle),
        )
      : rates;

    return [...filtered].sort((a, b) => {
      const result =
        sortKey === "rate" ? a.rate - b.rate : a.currencyCode.localeCompare(b.currencyCode);
      return ascending ? result : -result;
    });
  }, [rates, filter, sortKey, ascending]);

  function toggleSort(key: SortKey) {
    if (key === sortKey) setAscending((value) => !value);
    else {
      setSortKey(key);
      setAscending(true);
    }
  }

  function ariaSort(key: SortKey): "ascending" | "descending" | "none" {
    if (key !== sortKey) return "none";
    return ascending ? "ascending" : "descending";
  }

  return (
    <div>
      <div className="field">
        <label htmlFor="rate-filter">Filter currencies</label>
        <input
          id="rate-filter"
          type="search"
          placeholder="Search by code or name"
          value={filter}
          onChange={(event) => setFilter(event.target.value)}
        />
      </div>

      <div className="table-wrap">
        <table>
          <caption className="sr-only">Exchange rates against {baseCurrency}</caption>
          <thead>
            <tr>
              <th scope="col" aria-sort={ariaSort("currencyCode")}>
                <button type="button" className="sort-btn" onClick={() => toggleSort("currencyCode")}>
                  Currency
                </button>
              </th>
              <th scope="col" className="num" aria-sort={ariaSort("rate")}>
                <button type="button" className="sort-btn" onClick={() => toggleSort("rate")}>
                  1 {baseCurrency} =
                </button>
              </th>
            </tr>
          </thead>
          <tbody>
            {visible.map((rate) => (
              <tr key={rate.currencyCode}>
                <td>
                  <strong>{rate.currencyCode}</strong> <span className="muted">{rate.currencyName}</span>
                </td>
                <td className="num">{formatRate(rate.rate)}</td>
              </tr>
            ))}
            {visible.length === 0 && (
              <tr>
                <td colSpan={2} className="muted">
                  No currencies match "{filter}".
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
