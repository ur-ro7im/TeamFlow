import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

import Modal from "../../components/ui/Modal";
import TaskDetailModal from "../../components/tasks/TaskDetailModal";
import TaskForm from "../../components/tasks/TaskForm";
import { useAuth } from "../../context/useAuth";
import { getProjectMembers, getProjects } from "../../services/projectService";
import {
  createTask,
  deleteTask,
  getProjectTasks,
  updateTask,
  updateTaskStatus,
} from "../../services/taskService";
import { getErrorMessage } from "../../utils/apiError";
import { formatDate } from "../../utils/format";
import { canManageOrganization } from "../../utils/roles";
import {
  STATUS_LABELS,
  TASK_PRIORITIES,
  TASK_STATUSES,
} from "../../utils/taskConstants";
import "../projects/project.css";
import "./tasks.css";

const PAGE_SIZE = 10;

function TasksPage() {
  const { user } = useAuth();
  const canDelete = canManageOrganization(user?.role);

  const [searchParams, setSearchParams] = useSearchParams();

  // Projects
  const [projectsState, setProjectsState] = useState({
    loaded: false,
    items: [],
    error: "",
  });

  const projectParam = Number(searchParams.get("project")) || null;
  const projectId =
    projectsState.items.find((project) => project.id === projectParam)?.id ??
    projectsState.items[0]?.id ??
    null;

  // Filters
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [priority, setPriority] = useState("");
  const [page, setPage] = useState(1);
  const [reloadKey, setReloadKey] = useState(0);

  // Data
  const [membersState, setMembersState] = useState({ projectId: null, items: [] });
  const [result, setResult] = useState({
    key: "",
    items: [],
    totalPages: 1,
    totalCount: 0,
    error: "",
  });

  // UI
  const [pageError, setPageError] = useState("");
  const [formError, setFormError] = useState("");
  const [saving, setSaving] = useState(false);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [detailTask, setDetailTask] = useState(null);

  const queryKey = `${projectId}|${page}|${status}|${priority}|${search}|${reloadKey}`;
  const tasksLoading = Boolean(projectId) && result.key !== queryKey;
  const members =
    membersState.projectId === projectId ? membersState.items : [];

  useEffect(() => {
    let cancelled = false;

    getProjects()
      .then((data) => {
        if (cancelled) {
          return;
        }

        setProjectsState({
          loaded: true,
          items: Array.isArray(data) ? data : [],
          error: "",
        });
      })
      .catch((err) => {
        if (cancelled) {
          return;
        }

        setProjectsState({
          loaded: true,
          items: [],
          error: getErrorMessage(err, "Failed to load projects."),
        });
      });

    return () => {
      cancelled = true;
    };
  }, []);

  // Debounce the search box so the API is not called on every keystroke.
  useEffect(() => {
    const timer = setTimeout(() => {
      setSearch(searchInput.trim());
      setPage(1);
    }, 350);

    return () => clearTimeout(timer);
  }, [searchInput]);

  useEffect(() => {
    if (!projectId) {
      return undefined;
    }

    let cancelled = false;

    getProjectMembers(projectId)
      .then((data) => {
        if (!cancelled) {
          setMembersState({
            projectId,
            items: Array.isArray(data) ? data : [],
          });
        }
      })
      .catch(() => {
        if (!cancelled) {
          setMembersState({ projectId, items: [] });
        }
      });

    return () => {
      cancelled = true;
    };
  }, [projectId]);

  useEffect(() => {
    if (!projectId) {
      return undefined;
    }

    let cancelled = false;

    getProjectTasks(projectId, {
      search,
      status,
      priority,
      pageNumber: page,
      pageSize: PAGE_SIZE,
    })
      .then((data) => {
        if (cancelled) {
          return;
        }

        setResult({
          key: queryKey,
          items: Array.isArray(data?.items) ? data.items : [],
          totalPages: data?.totalPages || 1,
          totalCount: data?.totalCount ?? 0,
          error: "",
        });
      })
      .catch((err) => {
        if (cancelled) {
          return;
        }

        setResult({
          key: queryKey,
          items: [],
          totalPages: 1,
          totalCount: 0,
          error: getErrorMessage(err, "Failed to load tasks."),
        });
      });

    return () => {
      cancelled = true;
    };
  }, [projectId, search, status, priority, page, queryKey]);

  const reload = () => setReloadKey((key) => key + 1);

  const handleProjectChange = (event) => {
    setPage(1);
    setPageError("");
    setSearchParams({ project: event.target.value });
  };

  const closeForms = () => {
    setIsCreateOpen(false);
    setEditingTask(null);
    setFormError("");
  };

  const handleCreate = async (data) => {
    setSaving(true);
    setFormError("");

    try {
      await createTask(projectId, data);
      closeForms();
      setPage(1);
      reload();
    } catch (err) {
      setFormError(getErrorMessage(err, "Failed to create the task."));
    } finally {
      setSaving(false);
    }
  };

  const handleUpdate = async (data) => {
    setSaving(true);
    setFormError("");

    try {
      await updateTask(editingTask.id, data);
      closeForms();
      reload();
    } catch (err) {
      setFormError(getErrorMessage(err, "Failed to update the task."));
    } finally {
      setSaving(false);
    }
  };

  const handleStatusChange = async (task, newStatus) => {
    setPageError("");

    try {
      await updateTaskStatus(task.id, newStatus);
      reload();
    } catch (err) {
      setPageError(getErrorMessage(err, "Failed to update the task status."));
    }
  };

  const handleDelete = async (task) => {
    if (!window.confirm(`Delete "${task.title}"? This cannot be undone.`)) {
      return;
    }

    setPageError("");

    try {
      await deleteTask(task.id);
      reload();
    } catch (err) {
      setPageError(getErrorMessage(err, "Failed to delete the task."));
    }
  };

  if (!projectsState.loaded) {
    return <div className="page-loading">Loading tasks...</div>;
  }

  if (projectsState.error) {
    return <div className="error-message">{projectsState.error}</div>;
  }

  if (projectsState.items.length === 0) {
    return (
      <div>
        <div className="page-header">
          <h1>Tasks</h1>
          <p>Track and manage your team&apos;s work.</p>
        </div>

        <div className="empty-state">
          <div className="empty-state-icon">✅</div>
          <h2>No projects available</h2>
          <p>
            {canDelete
              ? "Create a project first, then add tasks to it."
              : "You have not been added to any project yet. Ask an admin to add you."}
          </p>
        </div>
      </div>
    );
  }

  const tasks = result.items;

  return (
    <div>
      <div className="page-header-row">
        <div>
          <h1>Tasks</h1>
          <p>Track and manage your team&apos;s work.</p>
        </div>

        <button
          className="primary-button"
          type="button"
          onClick={() => {
            setFormError("");
            setIsCreateOpen(true);
          }}
        >
          + New Task
        </button>
      </div>

      {pageError && <div className="error-message">{pageError}</div>}

      <div className="filters-bar">
        <select
          value={projectId ?? ""}
          onChange={handleProjectChange}
          aria-label="Project"
        >
          {projectsState.items.map((project) => (
            <option key={project.id} value={project.id}>
              {project.name}
            </option>
          ))}
        </select>

        <input
          type="search"
          placeholder="Search tasks..."
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
          aria-label="Search tasks"
        />

        <select
          value={status}
          onChange={(event) => {
            setStatus(event.target.value);
            setPage(1);
          }}
          aria-label="Status"
        >
          <option value="">All statuses</option>
          {TASK_STATUSES.map((value) => (
            <option key={value} value={value}>
              {STATUS_LABELS[value]}
            </option>
          ))}
        </select>

        <select
          value={priority}
          onChange={(event) => {
            setPriority(event.target.value);
            setPage(1);
          }}
          aria-label="Priority"
        >
          <option value="">All priorities</option>
          {TASK_PRIORITIES.map((value) => (
            <option key={value} value={value}>
              {value}
            </option>
          ))}
        </select>
      </div>

      {result.error && result.key === queryKey ? (
        <div>
          <div className="error-message">{result.error}</div>
          <button className="btn-refresh" type="button" onClick={reload}>
            Retry
          </button>
        </div>
      ) : tasksLoading ? (
        <div className="page-loading">Loading tasks...</div>
      ) : tasks.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">✅</div>
          <h2>No tasks found</h2>
          <p>
            {search || status || priority
              ? "Try changing the filters."
              : "Create the first task for this project."}
          </p>
          {page > 1 && (
            <button
              className="secondary-button"
              type="button"
              onClick={() => setPage(1)}
            >
              Back to the first page
            </button>
          )}
        </div>
      ) : (
        <>
          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Task</th>
                  <th>Status</th>
                  <th>Priority</th>
                  <th>Assignee</th>
                  <th>Due</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {tasks.map((task) => (
                  <tr key={task.id}>
                    <td>
                      <button
                        type="button"
                        className="task-title-button"
                        onClick={() => setDetailTask(task)}
                      >
                        {task.title}
                      </button>
                      <div className="task-counters">
                        <button
                          type="button"
                          className="counter-chip"
                          onClick={() => setDetailTask(task)}
                          title="Attachments"
                          aria-label={`${task.filesCount ?? 0} attachments`}
                        >
                          📎 {task.filesCount ?? 0}
                        </button>
                        <button
                          type="button"
                          className="counter-chip"
                          onClick={() => setDetailTask(task)}
                          title="Comments"
                          aria-label={`${task.commentsCount ?? 0} comments`}
                        >
                          💬 {task.commentsCount ?? 0}
                        </button>
                      </div>
                    </td>
                    <td>
                      <select
                        className="select-inline"
                        value={task.status}
                        onChange={(event) =>
                          handleStatusChange(task, event.target.value)
                        }
                        aria-label={`Status of ${task.title}`}
                      >
                        {TASK_STATUSES.map((value) => (
                          <option key={value} value={value}>
                            {STATUS_LABELS[value]}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td>
                      <span className={`badge badge-${task.priority}`}>
                        {task.priority}
                      </span>
                    </td>
                    <td>{task.assignedToUserName || "Unassigned"}</td>
                    <td>{formatDate(task.dueDate)}</td>
                    <td>
                      <div className="row-actions">
                        <button
                          type="button"
                          className="link-action"
                          onClick={() => setDetailTask(task)}
                        >
                          Details
                        </button>
                        <button
                          type="button"
                          className="link-action"
                          onClick={() => {
                            setFormError("");
                            setEditingTask(task);
                          }}
                        >
                          Edit
                        </button>
                        {canDelete && (
                          <button
                            type="button"
                            className="link-danger"
                            onClick={() => handleDelete(task)}
                          >
                            Delete
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="pagination">
            <span>
              {result.totalCount} task{result.totalCount === 1 ? "" : "s"} · Page{" "}
              {page} of {result.totalPages}
            </span>

            <div className="pagination-buttons">
              <button
                type="button"
                className="secondary-button"
                onClick={() => setPage((value) => Math.max(1, value - 1))}
                disabled={page <= 1}
              >
                Previous
              </button>
              <button
                type="button"
                className="secondary-button"
                onClick={() => setPage((value) => value + 1)}
                disabled={page >= result.totalPages}
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}

      <Modal isOpen={isCreateOpen} onClose={closeForms} title="Create Task">
        <TaskForm
          members={members}
          onSubmit={handleCreate}
          loading={saving}
          error={formError}
        />
      </Modal>

      <Modal isOpen={Boolean(editingTask)} onClose={closeForms} title="Edit Task">
        {editingTask && (
          <TaskForm
            key={editingTask.id}
            initialData={editingTask}
            members={members}
            onSubmit={handleUpdate}
            loading={saving}
            error={formError}
          />
        )}
      </Modal>

      {detailTask && (
        <TaskDetailModal
          key={detailTask.id}
          task={detailTask}
          user={user}
          onClose={() => {
            setDetailTask(null);
            reload();
          }}
        />
      )}
    </div>
  );
}

export default TasksPage;
