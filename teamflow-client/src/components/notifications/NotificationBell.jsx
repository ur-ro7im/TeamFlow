import { useEffect, useRef, useState } from "react";

import {
  getNotifications,
  getUnreadCount,
  markAllAsRead,
  markAsRead,
} from "../../services/notificationService";
import { getErrorMessage } from "../../utils/apiError";
import { formatDateTime } from "../../utils/format";
import "./notifications.css";

const POLL_INTERVAL_MS = 60_000;

function NotificationBell() {
  const [unreadCount, setUnreadCount] = useState(0);
  const [isOpen, setIsOpen] = useState(false);
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const wrapperRef = useRef(null);

  // Keep the badge fresh.
  useEffect(() => {
    let cancelled = false;

    const loadUnreadCount = async () => {
      try {
        const data = await getUnreadCount();
        if (!cancelled) {
          setUnreadCount(data?.count ?? 0);
        }
      } catch {
        // The badge is non-critical; keep the last known value.
      }
    };

    loadUnreadCount();
    const timer = setInterval(loadUnreadCount, POLL_INTERVAL_MS);

    return () => {
      cancelled = true;
      clearInterval(timer);
    };
  }, []);

  // Close on outside click / Escape.
  useEffect(() => {
    if (!isOpen) {
      return undefined;
    }

    const onPointerDown = (event) => {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    };

    const onKeyDown = (event) => {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);

    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [isOpen]);

  const loadNotifications = async () => {
    setLoading(true);
    setError("");

    try {
      const data = await getNotifications();
      const list = Array.isArray(data) ? data : [];
      setNotifications(list);
      setUnreadCount(list.filter((item) => !item.isRead).length);
    } catch (err) {
      setError(getErrorMessage(err, "Failed to load notifications."));
    } finally {
      setLoading(false);
    }
  };

  const togglePanel = () => {
    const next = !isOpen;
    setIsOpen(next);

    if (next) {
      loadNotifications();
    }
  };

  const handleMarkAsRead = async (notification) => {
    if (notification.isRead) {
      return;
    }

    try {
      await markAsRead(notification.id);
      setNotifications((prev) =>
        prev.map((item) =>
          item.id === notification.id ? { ...item, isRead: true } : item,
        ),
      );
      setUnreadCount((count) => Math.max(0, count - 1));
    } catch (err) {
      setError(getErrorMessage(err, "Failed to update the notification."));
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await markAllAsRead();
      setNotifications((prev) => prev.map((item) => ({ ...item, isRead: true })));
      setUnreadCount(0);
    } catch (err) {
      setError(getErrorMessage(err, "Failed to update notifications."));
    }
  };

  return (
    <div className="notification-wrapper" ref={wrapperRef}>
      <button
        className="notification-button"
        type="button"
        aria-label="Notifications"
        aria-expanded={isOpen}
        onClick={togglePanel}
      >
        🔔
        {unreadCount > 0 && (
          <span className="notification-badge">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="notification-panel" role="dialog" aria-label="Notifications">
          <div className="notification-panel-header">
            <h3>Notifications</h3>
            <button
              type="button"
              className="link-action"
              onClick={handleMarkAllAsRead}
              disabled={unreadCount === 0}
            >
              Mark all as read
            </button>
          </div>

          {error && <div className="notification-empty">{error}</div>}

          {loading ? (
            <div className="notification-empty">Loading...</div>
          ) : notifications.length === 0 && !error ? (
            <div className="notification-empty">You&apos;re all caught up.</div>
          ) : (
            <ul className="notification-list">
              {notifications.map((notification) => (
                <li key={notification.id}>
                  <button
                    type="button"
                    className={`notification-item${notification.isRead ? "" : " unread"}`}
                    onClick={() => handleMarkAsRead(notification)}
                  >
                    <p>{notification.message}</p>
                    <span>{formatDateTime(notification.createdAt)}</span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}

export default NotificationBell;
