import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { api, configureApi } from "../api/client";
import type { LoginPayload, RegisterPayload, TokenResponse } from "../api/types";

export interface Session {
  token: string;
  expiresAt: number; // epoch ms
  email: string;
  firstName: string;
  lastName: string;
}

interface AuthContextValue {
  session: Session | null;
  login: (payload: LoginPayload) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  logout: () => void;
}

const STORAGE_KEY = "cx.session";
const AuthContext = createContext<AuthContextValue | null>(null);

// sessionStorage keeps the token for the tab's lifetime only. A production app with
// stricter requirements would use an httpOnly cookie instead, to remove XSS exposure.
function loadSession(): Session | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as Session;
    if (!parsed.token || typeof parsed.expiresAt !== "number" || parsed.expiresAt <= Date.now()) {
      sessionStorage.removeItem(STORAGE_KEY);
      return null;
    }
    return parsed;
  } catch {
    sessionStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

function toSession(response: TokenResponse): Session {
  return {
    token: response.token,
    expiresAt: new Date(response.expiration).getTime(),
    email: response.email,
    firstName: response.firstName,
    lastName: response.lastName,
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(loadSession);

  const logout = useCallback(() => {
    sessionStorage.removeItem(STORAGE_KEY);
    setSession(null);
  }, []);

  const store = useCallback((response: TokenResponse) => {
    const next = toSession(response);
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    setSession(next);
  }, []);

  // Give the API client access to the current token and a global "sign out on 401".
  useEffect(() => {
    configureApi({
      getToken: () => session?.token ?? null,
      onUnauthorized: logout,
    });
  }, [session, logout]);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      logout,
      login: async (payload) => store(await api.login(payload)),
      register: async (payload) => store(await api.register(payload)),
    }),
    [session, logout, store],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used inside <AuthProvider>");
  return context;
}
