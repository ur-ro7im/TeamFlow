export function formatDate(value) {
  if (!value) {
    return "—";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "—";
  }

  return date.toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

export function formatDateTime(value) {
  if (!value) {
    return "—";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "—";
  }

  return date.toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function formatBytes(bytes) {
  if (!Number.isFinite(bytes) || bytes < 0) {
    return "—";
  }

  if (bytes === 0) {
    return "0 B";
  }

  const units = ["B", "KB", "MB", "GB", "TB"];
  const index = Math.min(
    Math.floor(Math.log(bytes) / Math.log(1024)),
    units.length - 1,
  );
  const value = bytes / 1024 ** index;

  return `${value.toFixed(index === 0 ? 0 : 1)} ${units[index]}`;
}

// Value for <input type="date"> from an ISO string.
export function toDateInputValue(value) {
  if (!value) {
    return "";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return date.toISOString().slice(0, 10);
}

// ISO string (or null) from an <input type="date"> value.
export function fromDateInputValue(value) {
  if (!value) {
    return null;
  }

  const date = new Date(`${value}T00:00:00.000Z`);

  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}

// Plan limits use -1 for "unlimited".
export function formatLimit(value) {
  return value < 0 ? "Unlimited" : String(value);
}

export function formatStorageLimit(bytes) {
  return bytes < 0 ? "Unlimited" : formatBytes(bytes);
}

export function formatPrice(price, currency = "USD") {
  if (!price) {
    return "Free";
  }

  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency,
  }).format(price);
}
