import { NavLink } from "react-router-dom";

function Sidebar({ isOpen, onClose }) {
  return (
    <>
      <div
        className={`sidebar-overlay ${isOpen ? "show" : ""}`}
        onClick={onClose}
      />

      <aside className={`sidebar ${isOpen ? "sidebar-open" : ""}`}>
        <div className="sidebar-header">
          <div className="sidebar-logo">TeamFlow</div>

          <button className="sidebar-close" type="button" onClick={onClose} aria-label="Close menu">
            ×
          </button>
        </div>

        <nav className="sidebar-nav">
          <NavLink to="/dashboard" onClick={onClose}>
            Dashboard
          </NavLink>

          <NavLink to="/projects" onClick={onClose}>
            Projects
          </NavLink>

          <NavLink to="/tasks" onClick={onClose}>
            Tasks
          </NavLink>

          <NavLink to="/members" onClick={onClose}>
            Members
          </NavLink>

          <NavLink to="/billing" onClick={onClose}>
            Billing
          </NavLink>
        </nav>
      </aside>
    </>
  );
}

export default Sidebar;
