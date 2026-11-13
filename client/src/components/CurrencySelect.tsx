import type { Currency } from "../api/types";

interface Props {
  id: string;
  label: string;
  value: string;
  currencies: Currency[];
  onChange: (code: string) => void;
}

export function CurrencySelect({ id, label, value, currencies, onChange }: Props) {
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <select id={id} value={value} onChange={(event) => onChange(event.target.value)}>
        {currencies.map((currency) => (
          <option key={currency.code} value={currency.code}>
            {currency.code} - {currency.name}
          </option>
        ))}
      </select>
    </div>
  );
}
