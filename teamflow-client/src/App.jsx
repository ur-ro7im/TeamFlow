import { BrowserRouter, Route, Routes } from "react-router-dom";

import DashboardLayout from "./components/layout/DashboardLayout";
import { AuthProvider } from "./context/AuthContext.jsx";
import LoginPage from "./pages/auth/LoginPage";
import AcceptInvitationPage from "./pages/invitations/AcceptInvitationPage";
import RegisterPage from "./pages/auth/RegisterPage";
import BillingPage from "./pages/billing/BillingPage";
import DashboardPage from "./pages/dashboard/DashboardPage";
import MembersPage from "./pages/members/MembersPage";
import ProjectsPage from "./pages/projects/ProjectsPage";
import TasksPage from "./pages/tasks/TasksPage";
import GuestRoute from "./routes/GuestRoute";
import HomeRedirect from "./routes/HomeRedirect";
import ProtectedRoute from "./routes/ProtectedRoute";

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/" element={<HomeRedirect />} />

          <Route path="/invite" element={<AcceptInvitationPage />} />

          <Route element={<GuestRoute />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>

          <Route element={<ProtectedRoute />}>
            <Route element={<DashboardLayout />}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/projects" element={<ProjectsPage />} />
              <Route path="/tasks" element={<TasksPage />} />
              <Route path="/members" element={<MembersPage />} />
              <Route path="/billing" element={<BillingPage />} />
            </Route>
          </Route>

          <Route path="*" element={<HomeRedirect />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
