import { useState } from "react";

import { fromDateInputValue, toDateInputValue } from "../../utils/format";

function ProjectForm({ initialData = null, onSubmit, loading = false, error = "" }) {
  const isEditMode = Boolean(initialData);

  const [form, setForm] = useState({
    name: initialData?.name ?? "",
    description: initialData?.description ?? "",
    startDate: toDateInputValue(initialData?.startDate),
    dueDate: toDateInputValue(initialData?.dueDate),
  });
  const [localError, setLocalError] = useState("");

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    setLocalError("");

    const name = form.name.trim();

    if (!name) {
      setLocalError("Project name is required.");
      return;
    }

    if (!isEditMode && form.startDate && form.dueDate && form.dueDate < form.startDate) {
      setLocalError("Due date cannot be before the start date.");
      return;
    }

    const payload = {
      name,
      description: form.description.trim() || null,
    };

    // The API only accepts dates when a project is created.
    if (!isEditMode) {
      payload.startDate = fromDateInputValue(form.startDate);
      payload.dueDate = fromDateInputValue(form.dueDate);
    }

    onSubmit(payload);
  };

  const message = localError || error;

  return (
    <form onSubmit={handleSubmit}>
      {message && <div className="error-message">{message}</div>}

      <div className="form-group">
        <label htmlFor="project-name">Project name</label>
        <input
          id="project-name"
          type="text"
          name="name"
          value={form.name}
          onChange={handleChange}
          placeholder="e.g. Website Redesign"
          maxLength={100}
          required
          disabled={loading}
        />
      </div>

      <div className="form-group">
        <label htmlFor="project-description">Description</label>
        <textarea
          id="project-description"
          name="description"
          value={form.description}
          onChange={handleChange}
          placeholder="Describe your project..."
          rows="4"
          maxLength={1000}
          disabled={loading}
        />
      </div>

      {!isEditMode && (
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="project-start">Start date</label>
            <input
              id="project-start"
              type="date"
              name="startDate"
              value={form.startDate}
              onChange={handleChange}
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="project-due">Due date</label>
            <input
              id="project-due"
              type="date"
              name="dueDate"
              value={form.dueDate}
              onChange={handleChange}
              disabled={loading}
            />
          </div>
        </div>
      )}

      <button type="submit" className="primary-button" disabled={loading}>
        {loading ? "Saving..." : isEditMode ? "Save Changes" : "Create Project"}
      </button>
    </form>
  );
}

export default ProjectForm;
