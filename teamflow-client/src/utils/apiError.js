export function getErrorMessage(error, fallback) {
  const response = error?.response;

  if (!response) {
    return error?.request
      ? "Cannot reach the server. Check your connection and try again."
      : fallback;
  }

  const data = response.data;

  if (typeof data === "string" && data.trim()) {
    return data;
  }

  if (data?.message && typeof data.message === "string") {
    return data.message;
  }

  // ASP.NET validation problem details: { errors: { Field: ["message"] } }
  if (data?.errors && typeof data.errors === "object") {
    const firstMessage = Object.values(data.errors)
      .flat()
      .find((message) => typeof message === "string");

    if (firstMessage) {
      return firstMessage;
    }
  }

  if (data?.title && typeof data.title === "string") {
    return data.title;
  }

  return fallback;
}
