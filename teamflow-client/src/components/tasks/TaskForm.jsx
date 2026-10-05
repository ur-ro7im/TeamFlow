import { useState } from "react";

import { fromDateInputValue, toDateInputValue } from "../../utils/format";
import { TASK_PRIORITIES } from "../../utils/taskConstants";

function TaskForm({ initialData = null, members = [], onSubmit, loading = false, error = "" }) {
  const isEditMode = Boolean(initialData);

  const [form, setForm] = useState({
    title: initialData?.title ?? "",
    description: initialData?.description ?? "",
    priority: initialData?.priority ?? "Medium",
    dueDate: toDateInputValue(initialData?.dueDate),
    assignedToUserId: initialData?.assignedToUserId
      ? String(initialData.assignedToUserId)
      : "",
  });
  const [localError, setLocalError] = useState("");

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    setLocalError("");

    const title = form.title.trim();

    if (!title) {
      setLocalError("Task title is required.");
      return;
    }

    onSubmit({
      title,
      description: form.description.trim() || null,
      priority: form.priority,
      dueDate: fromDateInputValue(form.dueDate),
      assignedToUserId: form.assignedToUserId
        ? Number(form.assignedToUserId)
        : null,
    });
  };

  const message = localError || error;

  return (
    <form onSubmit={handleSubmit}>
      {message && <div className="error-message">{message}</div>}

      <div className="form-group">
        <label htmlFor="task-title">Title</label>
        <input
          id="task-title"
          name="title"
          type="text"
          value={form.title}
          onChange={handleChange}
          maxLength={200}
          required
          disabled={loading}
        />
      </div>

      <div className="form-group">
        <label htmlFor="task-description">Description</label>
        <textarea
          id="task-description"
          name="description"
          rows="4"
          value={form.description}
          onChange={handleChange}
          maxLength={5000}
          disabled={loading}
        />
      </div>

      <div className="form-row">
        <div className="form-group">
          <label htmlFor="task-priority">Priority</label>
          <select
            id="task-priority"
            name="priority"
            value={form.priority}
            onChange={handleChange}
            disabled={loading}
          >
            {TASK_PRIORITIES.map((priority) => (
              <option key={priority} value={priority}>
                {priority}
              </option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label htmlFor="task-due">Due date</label>
          <input
            id="task-due"
            name="dueDate"
            type="date"
            value={form.dueDate}
            onChange={handleChange}
            disabled={loading}
          />
        </div>
      </div>

      <div className="form-group">
        <label htmlFor="task-assignee">Assignee</label>
        <select
          id="task-assignee"
          name="assignedToUserId"
          value={form.assignedToUserId}
          onChange={handleChange}
          disabled={loading}
        >
          <option value="">Unassigned</option>
          {members.map((member) => (
            <option key={member.userId} value={member.userId}>
              {member.fullName}
            </option>
          ))}
        </select>
        {members.length === 0 && (
          <p className="muted">
            Add members to this project to be able to assign tasks.
          </p>
        )}
      </div>

      <button type="submit" className="primary-button" disabled={loading}>
        {loading ? "Saving..." : isEditMode ? "Save Changes" : "Create Task"}
      </button>
    </form>
  );
}

export default TaskForm;
