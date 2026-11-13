import { Route, Routes } from "react-router-dom";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AuthForm } from "./AuthForm";
import { jsonResponse, mockFetch, renderWithProviders } from "../test/helpers";

const token = {
  token: "jwt",
  expiration: new Date(Date.now() + 3600_000).toISOString(),
  email: "ada@example.com",
  userId: "1",
  firstName: "Ada",
  lastName: "Obi",
};

function renderLogin() {
  return renderWithProviders(
    <Routes>
      <Route path="/login" element={<AuthForm mode="login" />} />
      <Route path="/" element={<p>dashboard</p>} />
    </Routes>,
    "/login",
  );
}

describe("login form", () => {
  it("shows validation messages without calling the API", async () => {
    const fetchMock = mockFetch({});
    renderLogin();
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(/email is required/i);
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("signs in, stores the session and navigates to the dashboard", async () => {
    mockFetch({ "/api/Auth/login": () => jsonResponse(token) });
    renderLogin();
    await userEvent.type(screen.getByLabelText("Email"), "ada@example.com");
    await userEvent.type(screen.getByLabelText("Password"), "Passw0rd");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByText("dashboard")).toBeInTheDocument();
    expect(JSON.parse(sessionStorage.getItem("cx.session")!).token).toBe("jwt");
  });

  it("shows the server's message when credentials are wrong", async () => {
    mockFetch({ "/api/Auth/login": () => jsonResponse({ error: "Invalid credentials" }, 401) });
    renderLogin();
    await userEvent.type(screen.getByLabelText("Email"), "ada@example.com");
    await userEvent.type(screen.getByLabelText("Password"), "Wrong1pass");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Invalid credentials");
    expect(screen.queryByText("dashboard")).not.toBeInTheDocument();
  });
});

describe("register form", () => {
  it("applies the password rules before submitting", async () => {
    const fetchMock = mockFetch({});
    renderWithProviders(<AuthForm mode="register" />);
    await userEvent.type(screen.getByLabelText("First name"), "Ada");
    await userEvent.type(screen.getByLabelText("Last name"), "Obi");
    await userEvent.type(screen.getByLabelText("Email"), "ada@example.com");
    await userEvent.type(screen.getByLabelText("Password"), "alllowercase1");
    await userEvent.click(screen.getByRole("button", { name: "Create account" }));

    await waitFor(() => expect(screen.getByRole("alert")).toHaveTextContent(/uppercase/i));
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
