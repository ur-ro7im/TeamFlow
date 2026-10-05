import { useState } from "react";
import { Link, useSearchParams } from "react-router-dom";

import { useAuth } from "../../context/useAuth";
import { acceptInvitation } from "../../services/organizationService";
import { getErrorMessage } from "../../utils/apiError";
import { clearPendingInvite, savePendingInvite } from "../../utils/authStorage";
import "../auth/login.css";

function AcceptInvitationPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token") ?? "";

  const { isAuthenticated, registerFromInvitation, logout } = useAuth();

  const [fullName, setFullName] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [accepted, setAccepted] = useState(false);

  if (!token) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <div className="auth-header">
            <h1>Invitation link is invalid</h1>
            <p>The invitation token is missing. Ask for a new invitation.</p>
          </div>
          <p className="auth-footer">
            <Link to="/login">Go to sign in</Link>
          </p>
        </div>
      </div>
    );
  }

  const handleRegister = async (event) => {
    event.preventDefault();
    setError("");
    setLoading(true);

    try {
      await registerFromInvitation({
        token,
        fullName: fullName.trim(),
        password,
      });
    } catch (err) {
      setError(getErrorMessage(err, "Could not create your account."));
    } finally {
      setLoading(false);
    }
  };

  const handleAccept = async () => {
    setError("");
    setLoading(true);

    try {
      await acceptInvitation(token);
      clearPendingInvite();
      setAccepted(true);
    } catch (err) {
      setError(getErrorMessage(err, "Could not accept the invitation."));
    } finally {
      setLoading(false);
    }
  };

  if (isAuthenticated) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <div className="auth-header">
            <h1>Organization invitation</h1>
            <p>
              {accepted
                ? "Invitation accepted. Sign out and sign in again to continue."
                : "Accept the invitation to join the organization."}
            </p>
          </div>

          {error && <div className="error-message">{error}</div>}

          {accepted ? (
            <button type="button" className="primary-button" onClick={logout}>
              Sign out
            </button>
          ) : (
            <>
              <button
                type="button"
                className="primary-button"
                onClick={handleAccept}
                disabled={loading}
              >
                {loading ? "Accepting..." : "Accept invitation"}
              </button>
              <p className="auth-footer">
                <Link to="/dashboard">Back to the dashboard</Link>
              </p>
            </>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-header">
          <h1>You&apos;ve been invited</h1>
          <p>Create your account to join the organization on TeamFlow.</p>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleRegister}>
          <div className="form-group">
            <label htmlFor="invite-full-name">Full name</label>
            <input
              id="invite-full-name"
              type="text"
              value={fullName}
              onChange={(event) => setFullName(event.target.value)}
              autoComplete="name"
              maxLength={100}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="invite-password">Password</label>
            <input
              id="invite-password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="new-password"
              minLength={8}
              maxLength={72}
              required
            />
          </div>

          <button type="submit" className="primary-button" disabled={loading}>
            {loading ? "Creating account..." : "Create account & join"}
          </button>
        </form>

        <p className="auth-footer">
          Already have an account?{" "}
          <Link to="/login" onClick={() => savePendingInvite(token)}>
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}

export default AcceptInvitationPage;
