import api from "../api/axiosClient";

// Public: used by the sign-up screen before an account exists.
export const getPlans = async () => {
  const response = await api.get("/Plans");
  return response.data;
};

export const getSubscription = async () => {
  const response = await api.get("/Billing/subscription");
  return response.data;
};

export const getPayments = async () => {
  const response = await api.get("/Billing/payments");
  return response.data;
};

export const changePlan = async (planId, card = null) => {
  const response = await api.post("/Billing/change-plan", { planId, card });
  return response.data;
};
