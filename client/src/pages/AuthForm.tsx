import { useState, type FormEvent } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { useAuth } from "../auth/AuthContext";
import { validateEmail, validateName, validatePassword } from "../validation";

type Mode = "login" | "register";

export function AuthForm({ mode }: { mode: Mode }) {
  const { login, register } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const destination = (location.state as { from?: string } | null)?.from ?? "/";

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);

  async function onSubmit(event: FormEvent) {
    event.preventDefault();

    const problems = [
      validateEmail(email),
      // Login only needs a non-empty password; the server judges the credentials.
      mode === "login" ? (password ? null : "Password is required") : validatePassword(password),
      ...(mode === "register" ? [validateName("First name", firstName), validateName("Last name", lastName)] : []),
    ].filter((problem): problem is string => problem !== null);

    if (problems.length > 0) {
      setErrors(problems);
      return;
    }

    setErrors([]);
    setSubmitting(true);
    try {
      if (mode === "login") await login({ email: email.trim(), password });
      else await register({ email: email.trim(), password, firstName: firstName.trim(), lastName: lastName.trim() });
      navigate(destination, { replace: true });
    } catch (error) {
      setErrors(error instanceof ApiError ? error.messages : ["Something went wrong. Please try again."]);
    } finally {
      setSubmitting(false);
    }
  }

  const isLogin = mode === "login";

  return (
    <div className="card narrow">
      <h1>{isLogin ? "Sign in" : "Create account"}</h1>
      <form onSubmit={onSubmit} noValidate>
        {!isLogin && (
          <div className="pair-row">
            <div className="field">
              <label htmlFor="firstName">First name</label>
              <input id="firstName" autoComplete="given-name" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
            </div>
            <div className="field">
              <label htmlFor="lastName">Last name</label>
              <input id="lastName" autoComplete="family-name" value={lastName} onChange={(e) => setLastName(e.target.value)} />
            </div>
          </div>
        )}

        <div className="field">
          <label htmlFor="email">Email</label>
          <input id="email" type="email" autoComplete="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>

        <div className="field">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            autoComplete={isLogin ? "current-password" : "new-password"}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          {!isLogin && <small className="muted">At least 6 characters with an uppercase letter, a lowercase letter and a digit.</small>}
        </div>

        <ErrorBanner messages={errors} />

        <button type="submit" className="btn" disabled={submitting}>
          {submitting ? "Please wait…" : isLogin ? "Sign in" : "Create account"}
        </button>
      </form>

      <p className="muted switch">
        {isLogin ? (
          <>
            No account? <Link to="/register">Create one</Link>
          </>
        ) : (
          <>
            Already registered? <Link to="/login">Sign in</Link>
          </>
        )}
      </p>
    </div>
  );
}
