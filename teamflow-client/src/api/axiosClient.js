import axios from "axios";

import { clearAuth, getAuth } from "../utils/authStorage";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "https://localhost:8018/api",
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use(
  (config) => {
    const auth = getAuth();

    if (auth?.token) {
      config.headers.Authorization = `Bearer ${auth.token}`;
    }

    return config;
  },
  (error) => Promise.reject(error),
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;
    const requestUrl = error?.config?.url ?? "";
    // Wrong credentials on the auth endpoints are reported by the form itself.
    const isAuthRequest = requestUrl.includes("/Auth/");

    if (status === 401 && !isAuthRequest) {
      clearAuth();

      if (window.location.pathname !== "/login") {
        window.location.assign("/login");
      }
    }

    return Promise.reject(error);
  },
);

export default api;
