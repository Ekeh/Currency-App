import { Link, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function Layout() {
  const { session, logout } = useAuth();
  const name = session ? `${session.firstName} ${session.lastName}`.trim() || session.email : "";

  return (
    <>
      <header className="app-header">
        <Link to="/" className="brand">
          Currency Exchange
        </Link>
        {session && (
          <div className="user-box">
            <span className="user-name" title={session.email}>
              {name}
            </span>
            <button type="button" className="btn btn-ghost" onClick={logout}>
              Sign out
            </button>
          </div>
        )}
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </>
  );
}
