import type { ReactElement } from "react";
import { render } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { vi } from "vitest";
import { AuthProvider, type Session } from "../auth/AuthContext";

export function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { "Content-Type": "application/json" } });
}

/** Routes fetch() calls by URL substring; unmatched calls fail the test loudly. */
export function mockFetch(routes: Record<string, () => Response | Promise<Response>>) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, _init?: RequestInit) => {
    const url = String(input);
    const key = Object.keys(routes).find((route) => url.includes(route));
    if (!key) throw new Error(`Unexpected fetch: ${url}`);
    return routes[key]();
  });
  vi.stubGlobal("fetch", fetchMock);
  return fetchMock;
}

export function signIn(overrides: Partial<Session> = {}) {
  const session: Session = {
    token: "test-token",
    expiresAt: Date.now() + 60 * 60 * 1000,
    email: "ada@example.com",
    firstName: "Ada",
    lastName: "Obi",
    ...overrides,
  };
  sessionStorage.setItem("cx.session", JSON.stringify(session));
  return session;
}

export function renderWithProviders(ui: ReactElement, route = "/") {
  return render(
    <MemoryRouter initialEntries={[route]}>
      <AuthProvider>{ui}</AuthProvider>
    </MemoryRouter>,
  );
}
