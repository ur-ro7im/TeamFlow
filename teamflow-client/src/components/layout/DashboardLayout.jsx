import { useState } from "react";
import { Outlet } from "react-router-dom";

import Sidebar from "./Sidebar";
import Topbar from "./Topbar";

function DashboardLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="dashboard-layout">
      <Sidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />

      <div className="main-area">
        <Topbar onMenuClick={() => setSidebarOpen(true)} />

        <main className="page-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default DashboardLayout;
