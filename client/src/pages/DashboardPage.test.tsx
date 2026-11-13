import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { DashboardPage } from "./DashboardPage";
import { jsonResponse, mockFetch, renderWithProviders, signIn } from "../test/helpers";

const currencies = {
  currencies: [
    { code: "NGN", name: "Nigerian Naira" },
    { code: "USD", name: "US Dollar" },
    { code: "EUR", name: "Euro" },
  ],
  count: 3,
};

const rates = (base: string) => ({
  baseCurrency: base,
  count: 2,
  lastUpdated: "2026-11-12T10:00:00Z",
  rates: [
    { currencyCode: "USD", currencyName: "US Dollar", rate: 0.00061, lastUpdated: "2026-11-12T10:00:00Z" },
    { currencyCode: "EUR", currencyName: "Euro", rate: 0.00056, lastUpdated: "2026-11-12T10:00:00Z" },
  ],
});

function routes() {
  return {
    "/currencies": () => jsonResponse(currencies),
    "/api/ExchangeRatesApi/NGN/USD": () =>
      jsonResponse({ from: "NGN", fromName: "Nigerian Naira", to: "USD", toName: "US Dollar", rate: 0.00061, lastUpdated: "2026-11-12T10:00:00Z" }),
    "baseCurrency=NGN": () => jsonResponse(rates("NGN")),
    "baseCurrency=USD": () => jsonResponse(rates("USD")),
  };
}

describe("DashboardPage", () => {
  beforeEach(() => signIn());

  it("loads currencies and renders the rates table", async () => {
    mockFetch(routes());
    renderWithProviders(<DashboardPage />);

    expect(screen.getByRole("status")).toHaveTextContent(/loading rates/i);
    const table = await screen.findByRole("table");
    expect(within(table).getByText("USD")).toBeInTheDocument();
    expect(within(table).getByText("EUR")).toBeInTheDocument();
  });

  it("filters rows by code or name", async () => {
    mockFetch(routes());
    renderWithProviders(<DashboardPage />);
    const table = await screen.findByRole("table");

    await userEvent.type(screen.getByLabelText("Filter currencies"), "euro");
    expect(within(table).queryByText("USD")).not.toBeInTheDocument();
    expect(within(table).getByText("EUR")).toBeInTheDocument();

    await userEvent.clear(screen.getByLabelText("Filter currencies"));
    await userEvent.type(screen.getByLabelText("Filter currencies"), "zzz");
    expect(within(table).getByText(/no currencies match/i)).toBeInTheDocument();
  });

  it("reloads rates when the base currency changes", async () => {
    const fetchMock = mockFetch(routes());
    renderWithProviders(<DashboardPage />);
    await screen.findByRole("table");

    await userEvent.selectOptions(await screen.findByLabelText("Base currency"), "USD");
    await screen.findByText(/1 USD =/);
    expect(fetchMock.mock.calls.some(([url]) => String(url).includes("baseCurrency=USD"))).toBe(true);
  });

  it("converts an amount using the pair rate", async () => {
    mockFetch(routes());
    renderWithProviders(<DashboardPage />);
    await screen.findByRole("table");

    await userEvent.selectOptions(await screen.findByLabelText("To"), "USD");
    await userEvent.clear(screen.getByLabelText("Amount"));
    await userEvent.type(screen.getByLabelText("Amount"), "1000");
    await userEvent.click(screen.getByRole("button", { name: "Convert" }));

    // 1000 NGN * 0.00061 = 0.61 USD
    expect(await screen.findByText(/0\.61 USD/)).toBeInTheDocument();
  });

  it("rejects a negative amount and identical currencies without calling the API", async () => {
    const fetchMock = mockFetch(routes());
    renderWithProviders(<DashboardPage />);
    await screen.findByRole("table");
    const callsBefore = fetchMock.mock.calls.length;

    await userEvent.clear(screen.getByLabelText("Amount"));
    await userEvent.type(screen.getByLabelText("Amount"), "-5");
    await userEvent.click(screen.getByRole("button", { name: "Convert" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(/amount/i);

    await userEvent.clear(screen.getByLabelText("Amount"));
    await userEvent.type(screen.getByLabelText("Amount"), "5");
    await userEvent.selectOptions(screen.getByLabelText("To"), "NGN");
    await userEvent.click(screen.getByRole("button", { name: "Convert" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(/two different currencies/i);

    expect(fetchMock.mock.calls.length).toBe(callsBefore);
  });

  it("shows an error banner when rates fail to load", async () => {
    mockFetch({
      ...routes(),
      "baseCurrency=NGN": () => jsonResponse({ error: "Upstream rate service unavailable" }, 400),
    });
    renderWithProviders(<DashboardPage />);
    expect(await screen.findByRole("alert")).toHaveTextContent("Upstream rate service unavailable");
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
  });
});
