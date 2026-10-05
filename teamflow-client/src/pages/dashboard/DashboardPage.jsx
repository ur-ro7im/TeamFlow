import { useEffect, useState } from "react";

import StatsCard from "../../components/ui/StatsCard";
import { useAuth } from "../../context/useAuth";
import { getDashboard } from "../../services/dashboardService";
import { getErrorMessage } from "../../utils/apiError";
import { formatDate } from "../../utils/format";
import { STATUS_LABELS } from "../../utils/taskConstants";
import "../projects/project.css";
import "../tasks/tasks.css";
import "./dashboard.css";

function getGreeting() {
  const hour = new Date().getHours();

  if (hour < 12) {
    return "Good morning";
  }

  if (hour < 18) {
    return "Good afternoon";
  }

  return "Good evening";
}

function DashboardPage() {
  const { user } = useAuth();

  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    let cancelled = false;

    getDashboard()
      .then((data) => {
        if (cancelled) {
          return;
        }

        setDashboard(data);
        setError("");
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) {
          return;
        }

        setError(getErrorMessage(err, "Failed to load dashboard."));
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [reloadKey]);

  const refresh = () => {
    setLoading(true);
    setReloadKey((key) => key + 1);
  };

  if (loading) {
    return (
      <div className="dashboard-loading">
        <div className="loading-spinner" />
        <p>Loading dashboard...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <div className="error-message">{error}</div>
        <button className="btn-refresh" onClick={refresh} type="button">
          Retry
        </button>
      </div>
    );
  }

  const stats = [
    { title: "Total Projects", value: dashboard?.projectsCount ?? 0 },
    { title: "Total Tasks", value: dashboard?.totalTasks ?? 0 },
    { title: "In Progress", value: dashboard?.inProgressTasks ?? 0 },
    { title: "Completed", value: dashboard?.completedTasks ?? 0 },
    { title: "Overdue", value: dashboard?.overdueTasks ?? 0 },
    { title: "Team Members", value: dashboard?.membersCount ?? 0 },
  ];

  const statistics = dashboard?.taskStatistics ?? {};
  const recentTasks = dashboard?.recentTasks ?? [];
  const firstName = user?.fullName?.split(" ")[0];

  return (
    <div>
      <div className="page-header">
        <h1>
          {getGreeting()}
          {firstName ? `, ${firstName}` : ""} 👋
        </h1>
        <p>Here&apos;s what&apos;s happening with your team today.</p>
      </div>

      <div className="stats-grid stats-grid-wide">
        {stats.map((stat) => (
          <StatsCard key={stat.title} title={stat.title} value={stat.value} />
        ))}
      </div>

      <div className="dashboard-columns">
        <section className="panel-block">
          <h2>Tasks by status</h2>
          <ul className="simple-list">
            {["todo", "inProgress", "review", "done"].map((key) => {
              const statusName = key.charAt(0).toUpperCase() + key.slice(1);

              return (
                <li key={key}>
                  <span>{STATUS_LABELS[statusName] ?? statusName}</span>
                  <strong>{statistics[key] ?? 0}</strong>
                </li>
              );
            })}
          </ul>
        </section>

        <section className="panel-block">
          <h2>Recent tasks</h2>
          {recentTasks.length === 0 ? (
            <p className="muted">No tasks yet.</p>
          ) : (
            <ul className="simple-list">
              {recentTasks.map((task) => (
                <li key={task.id}>
                  <div>
                    <strong>{task.title}</strong>
                    <div className="muted">
                      {task.projectName} · {formatDate(task.createdAt)}
                    </div>
                  </div>
                  <span className={`badge badge-${task.status}`}>
                    {STATUS_LABELS[task.status] ?? task.status}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>

      <button className="btn-refresh" onClick={refresh} type="button">
        Refresh
      </button>
    </div>
  );
}

export default DashboardPage;
