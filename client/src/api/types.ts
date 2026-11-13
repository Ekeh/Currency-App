// Mirrors the DTOs exposed by the .NET API (System.Text.Json, camelCase).

export interface TokenResponse {
  token: string;
  expiration: string;
  email: string;
  userId: string;
  firstName: string;
  lastName: string;
}

export interface Currency {
  code: string;
  name: string;
}

export interface SupportedCurrenciesResponse {
  currencies: Currency[];
  count: number;
}

export interface ExchangeRate {
  currencyCode: string;
  currencyName: string;
  rate: number;
  lastUpdated: string;
}

export interface ExchangeRatesResponse {
  baseCurrency: string;
  rates: ExchangeRate[];
  lastUpdated: string;
  count: number;
}

export interface CurrencyPairResponse {
  from: string;
  fromName: string;
  to: string;
  toName: string;
  rate: number;
  lastUpdated: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload extends LoginPayload {
  firstName: string;
  lastName: string;
}
