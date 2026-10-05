import {
  formatLimit,
  formatPrice,
  formatStorageLimit,
} from "../../utils/format";
import "./billing.css";

function PlanCard({
  plan,
  selected = false,
  current = false,
  recommended = false,
  actionLabel,
  onSelect,
  disabled = false,
}) {
  return (
    <article
      className={`plan-card${selected ? " is-selected" : ""}${current ? " is-current" : ""}`}
    >
      {recommended && <span className="plan-ribbon">Popular</span>}

      <h3>{plan.name}</h3>

      <div className="plan-price">
        <strong>{formatPrice(plan.price, plan.currency)}</strong>
        {!plan.isFree && <span> / month</span>}
      </div>

      <ul className="plan-features">
        <li>{formatLimit(plan.maxMembers)} members</li>
        <li>{formatLimit(plan.maxProjects)} projects</li>
        <li>{formatStorageLimit(plan.maxStorage)} storage</li>
      </ul>

      {current ? (
        <span className="badge">Current plan</span>
      ) : (
        onSelect && (
          <button
            type="button"
            className={selected ? "primary-button" : "secondary-button"}
            onClick={() => onSelect(plan)}
            disabled={disabled}
          >
            {actionLabel ?? (selected ? "Selected" : "Select")}
          </button>
        )
      )}
    </article>
  );
}

export default PlanCard;
