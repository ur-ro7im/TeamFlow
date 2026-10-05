import { useAuth } from "../../context/useAuth";
import NotificationBell from "../notifications/NotificationBell";

function Topbar({ onMenuClick }) {
  const { user, logout } = useAuth();

  const initial = user?.fullName?.charAt(0)?.toUpperCase() || "?";

  return (
    <header className="topbar">
      <button
        className="mobile-menu-button"
        type="button"
        onClick={onMenuClick}
        aria-label="Open menu"
      >
        ☰
      </button>

      <div className="topbar-spacer" />

      <NotificationBell />

      <div className="avatar" title={user?.fullName}>
        {initial}
      </div>

      <button onClick={logout} className="logout-button" type="button">
        Logout
      </button>
    </header>
  );
}

export default Topbar;
