import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

import CheckoutModal from "../../components/billing/CheckoutModal";
import PlanCard from "../../components/billing/PlanCard";
import { useAuth } from "../../context/useAuth";
import {
  changePlan,
  getPayments,
  getPlans,
  getSubscription,
} from "../../services/billingService";
import { getErrorMessage } from "../../utils/apiError";
import {
  formatBytes,
  formatDate,
  formatDateTime,
  formatLimit,
  formatPrice,
  formatStorageLimit,
} from "../../utils/format";
import { isOwner } from "../../utils/roles";
import "../members/members.css";
import "../tasks/tasks.css";
import "../../components/billing/billing.css";

function UsageRow({ label, used, limit, formatUsed, formatMax }) {
  const unlimited = limit < 0;
  const percent = unlimited || limit === 0 ? 0 : Math.min(100, (used / limit) * 100);

  return (
    <div className="usage-row">
      <div className="usage-row-label">
        <span>{label}</span>
        <span className="muted">
          {formatUsed(used)} / {formatMax(limit)}
        </span>
      </div>
      <div className={`storage-bar${percent >= 100 ? " is-full" : ""}`}>
        <div style={{ width: `${percent}%` }} />
      </div>
    </div>
  );
}

function BillingPage() {
  const { user } = useAuth();
  const owner = isOwner(user?.role);

  const [searchParams, setSearchParams] = useSearchParams();
  const checkoutPlanId = Number(searchParams.get("checkout")) || null;

  const [state, setState] = useState({
    loaded: false,
    subscription: null,
    plans: [],
    payments: [],
    error: "",
  });
  const [reloadKey, setReloadKey] = useState(0);

  const [checkoutPlan, setCheckoutPlan] = useState(null);
  const [checkoutDismissed, setCheckoutDismissed] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [actionError, setActionError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    let cancelled = false;

    Promise.all([
      getSubscription(),
      getPlans(),
      owner ? getPayments() : Promise.resolve([]),
    ])
      .then(([subscription, plans, payments]) => {
        if (!cancelled) {
          setState({
            loaded: true,
            subscription,
            plans: Array.isArray(plans) ? plans : [],
            payments: Array.isArray(payments) ? payments : [],
            error: "",
          });
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setState((prev) => ({
            ...prev,
            loaded: true,
            error: getErrorMessage(err, "Failed to load billing information."),
          }));
        }
      });

    return () => {
      cancelled = true;
    };
  }, [owner, reloadKey]);

  const reload = () => setReloadKey((key) => key + 1);

  const currentPlanId = state.subscription?.plan?.id;

  // A plan picked on the sign-up screen opens its checkout straight away.
  const autoCheckoutPlan =
    owner && !checkoutDismissed && checkoutPlanId && checkoutPlanId !== currentPlanId
      ? state.plans.find((plan) => plan.id === checkoutPlanId && !plan.isFree)
      : null;

  const activeCheckoutPlan = checkoutPlan ?? autoCheckoutPlan ?? null;

  const closeCheckout = () => {
    setCheckoutPlan(null);
    setCheckoutDismissed(true);
    setActionError("");

    if (searchParams.has("checkout")) {
      setSearchParams({}, { replace: true });
    }
  };

  const applyPlan = async (plan, card) => {
    setProcessing(true);
    setActionError("");
    setSuccess("");

    try {
      await changePlan(plan.id, card);
      setSuccess(`Your organization is now on the ${plan.name} plan.`);
      closeCheckout();
      reload();
    } catch (err) {
      setActionError(getErrorMessage(err, "Could not change the plan."));
    } finally {
      setProcessing(false);
    }
  };

  const handleSelect = (plan) => {
    setSuccess("");
    setActionError("");

    if (plan.isFree) {
      const confirmed = window.confirm(
        `Switch to the ${plan.name} plan? Existing data is kept, but you will not be able to add more than the plan allows.`,
      );

      if (confirmed) {
        applyPlan(plan, null);
      }

      return;
    }

    setCheckoutPlan(plan);
  };

  if (!state.loaded) {
    return <div className="page-loading">Loading billing...</div>;
  }

  if (state.error && !state.subscription) {
    return (
      <div>
        <div className="error-message">{state.error}</div>
        <button className="btn-refresh" type="button" onClick={reload}>
          Retry
        </button>
      </div>
    );
  }

  const { subscription, plans, payments } = state;
  const plan = subscription.plan;
  const usage = subscription.usage;

  return (
    <div>
      <div className="page-header">
        <h1>Billing</h1>
        <p>Your organization&apos;s plan, usage and payments.</p>
      </div>

      {success && <div className="success-message">{success}</div>}
      {!activeCheckoutPlan && actionError && (
        <div className="error-message">{actionError}</div>
      )}

      <div className="members-layout" style={{ marginBottom: 24 }}>
        <section className="panel">
          <h2>
            Current plan: {plan.name}{" "}
            <span className="badge">{formatPrice(plan.price, plan.currency)}</span>
          </h2>
          <p className="muted" style={{ marginTop: -6 }}>
            {plan.isFree
              ? "Free plan"
              : `Renews on ${formatDate(subscription.endDate)} (moves back to Free if not renewed)`}
          </p>

          <UsageRow
            label="Members"
            used={usage.members}
            limit={plan.maxMembers}
            formatUsed={String}
            formatMax={formatLimit}
          />
          <UsageRow
            label="Projects"
            used={usage.projects}
            limit={plan.maxProjects}
            formatUsed={String}
            formatMax={formatLimit}
          />
          <UsageRow
            label="Storage"
            used={usage.storageBytes}
            limit={plan.maxStorage}
            formatUsed={formatBytes}
            formatMax={formatStorageLimit}
          />
        </section>

        <aside className="panel">
          <h2>Payment history</h2>
          {!owner ? (
            <p className="muted">Only the organization owner can see payments.</p>
          ) : payments.length === 0 ? (
            <p className="muted">No payments yet.</p>
          ) : (
            <ul className="simple-list">
              {payments.map((payment) => (
                <li key={payment.id}>
                  <div>
                    <strong>{formatPrice(payment.amount, payment.currency)}</strong>
                    <div className="muted">{formatDateTime(payment.createdAt)}</div>
                  </div>
                  <span
                    className={`badge ${payment.status === "Succeeded" ? "badge-Done" : "badge-Urgent"}`}
                  >
                    {payment.status}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </aside>
      </div>

      <h2>{owner ? "Change plan" : "Available plans"}</h2>
      {!owner && (
        <p className="muted">Only the organization owner can change the plan.</p>
      )}

      <div className="plans-grid">
        {plans.map((item) => (
          <PlanCard
            key={item.id}
            plan={item}
            current={item.id === currentPlanId}
            recommended={item.name === "Pro"}
            onSelect={owner ? handleSelect : undefined}
            actionLabel={
              plan.price < item.price ? `Upgrade to ${item.name}` : `Switch to ${item.name}`
            }
            disabled={processing}
          />
        ))}
      </div>

      <CheckoutModal
        key={activeCheckoutPlan?.id ?? "none"}
        plan={activeCheckoutPlan}
        onClose={closeCheckout}
        onConfirm={(card) => applyPlan(activeCheckoutPlan, card)}
        loading={processing}
        error={actionError}
      />
    </div>
  );
}

export default BillingPage;
