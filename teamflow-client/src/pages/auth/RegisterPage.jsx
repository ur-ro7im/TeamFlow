import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import PlanCard from "../../components/billing/PlanCard";
import { useAuth } from "../../context/useAuth";
import { getPlans } from "../../services/billingService";
import { getErrorMessage } from "../../utils/apiError";
import "./login.css";

function RegisterPage() {
  const { register } = useAuth();

  const [step, setStep] = useState("plan");
  const [plans, setPlans] = useState([]);
  const [plansLoading, setPlansLoading] = useState(true);
  const [plansError, setPlansError] = useState("");
  const [selectedPlan, setSelectedPlan] = useState(null);

  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    organizationName: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    getPlans()
      .then((data) => {
        if (cancelled) {
          return;
        }

        const list = Array.isArray(data) ? data : [];
        setPlans(list);
        setSelectedPlan((current) => current ?? list.find((p) => p.isFree) ?? list[0] ?? null);
        setPlansLoading(false);
      })
      .catch((err) => {
        if (cancelled) {
          return;
        }

        setPlansError(getErrorMessage(err, "Failed to load plans."));
        setPlansLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setLoading(true);

    try {
      await register({
        ...form,
        fullName: form.fullName.trim(),
        email: form.email.trim(),
        organizationName: form.organizationName.trim(),
        planId: selectedPlan?.id ?? null,
      });
    } catch (err) {
      setError(getErrorMessage(err, "Registration failed. Please try again."));
    } finally {
      setLoading(false);
    }
  };

  if (step === "plan") {
    return (
      <div className="auth-page">
        <div className="auth-card auth-card-wide">
          <div className="auth-header">
            <h1>Choose your plan</h1>
            <p>You can change it later from the Billing page.</p>
          </div>

          {plansError && <div className="error-message">{plansError}</div>}

          {plansLoading ? (
            <p className="muted">Loading plans...</p>
          ) : (
            <div className="plans-grid">
              {plans.map((plan) => (
                <PlanCard
                  key={plan.id}
                  plan={plan}
                  selected={selectedPlan?.id === plan.id}
                  recommended={plan.name === "Pro"}
                  onSelect={setSelectedPlan}
                />
              ))}
            </div>
          )}

          <button
            type="button"
            className="primary-button"
            onClick={() => setStep("details")}
            disabled={plansLoading || (plans.length > 0 && !selectedPlan)}
          >
            Continue{selectedPlan ? ` with ${selectedPlan.name}` : ""}
          </button>

          <p className="auth-footer">
            Already have an account? <Link to="/login">Sign in</Link>
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-header">
          <h1>Create your account</h1>
          <p>
            Plan: <strong>{selectedPlan?.name ?? "Free"}</strong>{" "}
            <button
              type="button"
              className="link-action"
              onClick={() => setStep("plan")}
            >
              Change
            </button>
          </p>
        </div>

        {error && <div className="error-message">{error}</div>}

        {selectedPlan && !selectedPlan.isFree && (
          <p className="card-note">
            You will start on the Free plan and enter your payment details right
            after creating the account to activate {selectedPlan.name}.
          </p>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="register-fullName">Full name</label>
            <input
              id="register-fullName"
              type="text"
              name="fullName"
              value={form.fullName}
              onChange={handleChange}
              autoComplete="name"
              maxLength={100}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="register-email">Email</label>
            <input
              id="register-email"
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              autoComplete="email"
              maxLength={255}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="register-organization">Organization name</label>
            <input
              id="register-organization"
              type="text"
              name="organizationName"
              value={form.organizationName}
              onChange={handleChange}
              autoComplete="organization"
              maxLength={100}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="register-password">Password</label>
            <input
              id="register-password"
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              autoComplete="new-password"
              minLength={8}
              maxLength={72}
              required
            />
          </div>

          <button type="submit" className="primary-button" disabled={loading}>
            {loading ? "Creating account..." : "Create account"}
          </button>
        </form>

        <p className="auth-footer">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </div>
    </div>
  );
}

export default RegisterPage;
