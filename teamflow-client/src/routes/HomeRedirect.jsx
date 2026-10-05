import { Navigate } from "react-router-dom";

import { useAuth } from "../context/useAuth";

function HomeRedirect() {
  const { isAuthenticated } = useAuth();

  return (
    <Navigate to={isAuthenticated ? "/dashboard" : "/login"} replace />
  );
}

export default HomeRedirect;
