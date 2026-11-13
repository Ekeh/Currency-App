import type {
  CurrencyPairResponse,
  ExchangeRatesResponse,
  LoginPayload,
  RegisterPayload,
  SupportedCurrenciesResponse,
  TokenResponse,
} from "./types";

/** Error raised for any non-2xx response, carrying user-presentable messages. */
export class ApiError extends Error {
  readonly status: number;
  readonly messages: string[];

  constructor(status: number, messages: string[]) {
    super(messages[0] ?? "Request failed");
    this.name = "ApiError";
    this.status = status;
    this.messages = messages;
  }
}

interface ApiConfig {
  getToken: () => string | null;
  onUnauthorized: () => void;
}

let config: ApiConfig = { getToken: () => null, onUnauthorized: () => {} };

/** Wires the client to the auth layer without importing React into this module. */
export function configureApi(next: ApiConfig): void {
  config = next;
}

interface RequestOptions {
  method?: "GET" | "POST";
  body?: unknown;
  /** Set false for endpoints that must not trigger a global sign-out on 401 (login). */
  authenticated?: boolean;
  signal?: AbortSignal;
}

/** The API returns { errors: string[] } or { error: string } (either casing). */
function extractMessages(status: number, body: unknown): string[] {
  if (body && typeof body === "object") {
    const record = body as Record<string, unknown>;
    const list = record.errors ?? record.Errors;
    if (Array.isArray(list) && list.length > 0) return list.map(String);
    const single = record.error ?? record.Error;
    if (typeof single === "string" && single) return [single];
  }
  if (status === 401) return ["Your session has expired. Please sign in again."];
  if (status === 429) return ["Too many requests. Please wait a moment and try again."];
  if (status >= 500) return ["The server had a problem. Please try again shortly."];
  return ["Something went wrong. Please try again."];
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, authenticated = true, signal } = options;

  const headers: Record<string, string> = { Accept: "application/json" };
  if (body !== undefined) headers["Content-Type"] = "application/json";

  const token = authenticated ? config.getToken() : null;
  if (token) headers.Authorization = `Bearer ${token}`;

  let response: Response;
  try {
    response = await fetch(path, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      signal,
    });
  } catch (error) {
    if (error instanceof DOMException && error.name === "AbortError") throw error;
    throw new ApiError(0, ["Cannot reach the server. Check your connection and that the API is running."]);
  }

  if (response.ok) {
    return (await response.json()) as T;
  }

  let payload: unknown = null;
  try {
    payload = await response.json();
  } catch {
    // Non-JSON error body (e.g. a 503 from a proxy); fall back to status messages.
  }

  if (response.status === 401 && authenticated) config.onUnauthorized();
  throw new ApiError(response.status, extractMessages(response.status, payload));
}

export const api = {
  login: (payload: LoginPayload) =>
    request<TokenResponse>("/api/Auth/login", { method: "POST", body: payload, authenticated: false }),

  register: (payload: RegisterPayload) =>
    request<TokenResponse>("/api/Auth/register", { method: "POST", body: payload, authenticated: false }),

  getCurrencies: (signal?: AbortSignal) =>
    request<SupportedCurrenciesResponse>("/api/ExchangeRatesApi/currencies", { signal }),

  getRates: (baseCurrency: string, signal?: AbortSignal) =>
    request<ExchangeRatesResponse>(
      `/api/ExchangeRatesApi?baseCurrency=${encodeURIComponent(baseCurrency)}`,
      { signal },
    ),

  getPair: (from: string, to: string, signal?: AbortSignal) =>
    request<CurrencyPairResponse>(
      `/api/ExchangeRatesApi/${encodeURIComponent(from)}/${encodeURIComponent(to)}`,
      { signal },
    ),
};
