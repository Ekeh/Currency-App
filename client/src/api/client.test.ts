import { api, ApiError, configureApi } from "./client";
import { jsonResponse, mockFetch } from "../test/helpers";

describe("api client", () => {
  const onUnauthorized = vi.fn();

  beforeEach(() => {
    onUnauthorized.mockReset();
    configureApi({ getToken: () => "abc123", onUnauthorized });
  });

  it("sends the bearer token on authenticated calls", async () => {
    const fetchMock = mockFetch({ "/currencies": () => jsonResponse({ currencies: [], count: 0 }) });
    await api.getCurrencies();
    const init = fetchMock.mock.calls[0][1] as RequestInit;
    expect((init.headers as Record<string, string>).Authorization).toBe("Bearer abc123");
  });

  it("does not send a token to the login endpoint", async () => {
    const fetchMock = mockFetch({ "/login": () => jsonResponse({ token: "t" }) });
    await api.login({ email: "a@b.co", password: "x" });
    const init = fetchMock.mock.calls[0][1] as RequestInit;
    expect((init.headers as Record<string, string>).Authorization).toBeUndefined();
  });

  it("URL-encodes the base currency", async () => {
    const fetchMock = mockFetch({ "/api/ExchangeRatesApi": () => jsonResponse({ rates: [] }) });
    await api.getRates("N G");
    expect(String(fetchMock.mock.calls[0][0])).toContain("baseCurrency=N%20G");
  });

  it("turns { errors: [...] } into an ApiError with every message", async () => {
    mockFetch({ "/login": () => jsonResponse({ errors: ["Email is required", "Password is required"] }, 400) });
    const error = await api.login({ email: "", password: "" }).catch((e) => e);
    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(400);
    expect(error.messages).toEqual(["Email is required", "Password is required"]);
  });

  it("reads the single { error } shape as well", async () => {
    mockFetch({ "/login": () => jsonResponse({ error: "Invalid credentials" }, 401) });
    const error = await api.login({ email: "a@b.co", password: "x" }).catch((e) => e);
    expect(error.messages).toEqual(["Invalid credentials"]);
  });

  it("signs the user out on a 401 from an authenticated call", async () => {
    mockFetch({ "/currencies": () => jsonResponse({}, 401) });
    await api.getCurrencies().catch(() => undefined);
    expect(onUnauthorized).toHaveBeenCalledOnce();
  });

  it("does NOT sign out when the login call itself returns 401", async () => {
    mockFetch({ "/login": () => jsonResponse({ error: "Invalid credentials" }, 401) });
    await api.login({ email: "a@b.co", password: "x" }).catch(() => undefined);
    expect(onUnauthorized).not.toHaveBeenCalled();
  });

  it("gives a friendly message when the rate limiter rejects the call", async () => {
    mockFetch({ "/currencies": () => new Response("slow down", { status: 429 }) });
    const error = await api.getCurrencies().catch((e) => e);
    expect(error.messages[0]).toMatch(/too many requests/i);
  });

  it("reports an unreachable server clearly", async () => {
    vi.stubGlobal("fetch", vi.fn().mockRejectedValue(new TypeError("Failed to fetch")));
    const error = await api.getCurrencies().catch((e) => e);
    expect(error.status).toBe(0);
    expect(error.messages[0]).toMatch(/cannot reach the server/i);
  });
});
