import { useState } from "react";
import { useNavigate } from "react-router-dom";

import {
  login as loginRequest,
  register as registerRequest,
  registerFromInvitation as registerFromInvitationRequest,
} from "../services/authService";
import {
  clearAuth,
  clearPendingInvite,
  getAuth,
  getPendingInvite,
  saveAuth,
} from "../utils/authStorage";
import { AuthContext } from "./auth-context";

export function AuthProvider({ children }) {
  const navigate = useNavigate();

  const [auth, setAuth] = useState(() => getAuth());

  const startSession = (result) => {
    saveAuth(result);
    setAuth(result);
  };

  const login = async (credentials) => {
    const result = await loginRequest(credentials);
    startSession(result);

    // Someone who opened an invitation link before signing in goes back to it.
    const pendingInvite = getPendingInvite();
    navigate(
      pendingInvite
        ? `/invite?token=${encodeURIComponent(pendingInvite)}`
        : "/dashboard",
      { replace: true },
    );

    return result;
  };

  const register = async (data) => {
    const result = await registerRequest(data);
    startSession(result);

    // A paid plan is activated only after payment, so continue to checkout.
    navigate(
      result.requiresPayment && result.selectedPlanId
        ? `/billing?checkout=${result.selectedPlanId}`
        : "/dashboard",
      { replace: true },
    );

    return result;
  };

  const registerFromInvitation = async (data) => {
    const result = await registerFromInvitationRequest(data);
    clearPendingInvite();
    startSession(result);
    navigate("/dashboard", { replace: true });
    return result;
  };

  const logout = () => {
    clearAuth();
    setAuth(null);
    navigate("/login", { replace: true });
  };

  return (
    <AuthContext.Provider
      value={{
        auth,
        user: auth,
        isAuthenticated: Boolean(auth?.token),
        login,
        register,
        registerFromInvitation,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
