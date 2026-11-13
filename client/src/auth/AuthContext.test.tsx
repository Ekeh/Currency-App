import { screen } from "@testing-library/react";
import { useAuth } from "./AuthContext";
import { renderWithProviders, signIn } from "../test/helpers";

function Probe() {
  const { session, logout } = useAuth();
  return (
    <div>
      <p data-testid="who">{session ? session.email : "anonymous"}</p>
      <button onClick={logout}>logout</button>
    </div>
  );
}

describe("AuthProvider", () => {
  it("restores a valid session from sessionStorage", () => {
    signIn({ email: "restored@example.com" });
    renderWithProviders(<Probe />);
    expect(screen.getByTestId("who")).toHaveTextContent("restored@example.com");
  });

  it("discards an expired session", () => {
    signIn({ expiresAt: Date.now() - 1000 });
    renderWithProviders(<Probe />);
    expect(screen.getByTestId("who")).toHaveTextContent("anonymous");
    expect(sessionStorage.getItem("cx.session")).toBeNull();
  });

  it("discards corrupt stored data instead of crashing", () => {
    sessionStorage.setItem("cx.session", "{not json");
    renderWithProviders(<Probe />);
    expect(screen.getByTestId("who")).toHaveTextContent("anonymous");
  });

  it("logout clears the session", async () => {
    signIn();
    const { default: userEvent } = await import("@testing-library/user-event");
    renderWithProviders(<Probe />);
    await userEvent.click(screen.getByRole("button", { name: "logout" }));
    expect(screen.getByTestId("who")).toHaveTextContent("anonymous");
    expect(sessionStorage.getItem("cx.session")).toBeNull();
  });
});
