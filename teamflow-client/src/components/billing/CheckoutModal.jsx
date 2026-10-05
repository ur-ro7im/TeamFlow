import { useState } from "react";

import Modal from "../ui/Modal";
import { formatPrice } from "../../utils/format";

const currentYear = new Date().getFullYear();

function CheckoutModal({ plan, onClose, onConfirm, loading, error }) {
  const [form, setForm] = useState({
    cardHolder: "",
    cardNumber: "",
    expiryMonth: "12",
    expiryYear: String(currentYear + 2),
    cvc: "",
  });

  const handleChange = (event) => {
    const { name, value } = event.target;

    setForm((prev) => ({
      ...prev,
      [name]: name === "cardNumber" ? value.replace(/[^\d ]/g, "") : value,
    }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();

    onConfirm({
      cardHolder: form.cardHolder.trim(),
      cardNumber: form.cardNumber,
      expiryMonth: Number(form.expiryMonth),
      expiryYear: Number(form.expiryYear),
      cvc: form.cvc,
    });
  };

  return (
    <Modal
      isOpen={Boolean(plan)}
      onClose={loading ? () => {} : onClose}
      title={plan ? `Upgrade to ${plan.name}` : "Checkout"}
    >
      {plan && (
        <form onSubmit={handleSubmit}>
          <p className="card-note">
            Demo payment gateway — no real charge is made. Use card{" "}
            <strong>4242 4242 4242 4242</strong> to succeed or{" "}
            <strong>4000 0000 0000 0002</strong> to see a declined payment.
          </p>

          {error && <div className="error-message">{error}</div>}

          <div className="form-group">
            <label htmlFor="card-holder">Name on card</label>
            <input
              id="card-holder"
              name="cardHolder"
              type="text"
              value={form.cardHolder}
              onChange={handleChange}
              autoComplete="cc-name"
              maxLength={100}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="card-number">Card number</label>
            <input
              id="card-number"
              name="cardNumber"
              type="text"
              inputMode="numeric"
              value={form.cardNumber}
              onChange={handleChange}
              autoComplete="cc-number"
              placeholder="4242 4242 4242 4242"
              maxLength={23}
              required
              disabled={loading}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="card-month">Expiry month</label>
              <select
                id="card-month"
                name="expiryMonth"
                value={form.expiryMonth}
                onChange={handleChange}
                disabled={loading}
              >
                {Array.from({ length: 12 }, (_, i) => i + 1).map((month) => (
                  <option key={month} value={month}>
                    {String(month).padStart(2, "0")}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="card-year">Expiry year</label>
              <select
                id="card-year"
                name="expiryYear"
                value={form.expiryYear}
                onChange={handleChange}
                disabled={loading}
              >
                {Array.from({ length: 11 }, (_, i) => currentYear + i).map(
                  (year) => (
                    <option key={year} value={year}>
                      {year}
                    </option>
                  ),
                )}
              </select>
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="card-cvc">Security code</label>
            <input
              id="card-cvc"
              name="cvc"
              type="password"
              inputMode="numeric"
              value={form.cvc}
              onChange={handleChange}
              autoComplete="cc-csc"
              maxLength={4}
              required
              disabled={loading}
            />
          </div>

          <button type="submit" className="primary-button" disabled={loading}>
            {loading
              ? "Processing..."
              : `Pay ${formatPrice(plan.price, plan.currency)} / month`}
          </button>
        </form>
      )}
    </Modal>
  );
}

export default CheckoutModal;
