import { useState } from "react";

import { formatDate } from "../../utils/format";
import "./ProjectCard.css";

function ProjectCard({
  project,
  canManage,
  onOpen,
  onEdit,
  onDelete,
  onMembers,
}) {
  const [showActions, setShowActions] = useState(false);

  const runAction = (action) => {
    setShowActions(false);
    action(project);
  };

  return (
    <article className="project-card">
      <div className="project-card-header">
        <div className="project-icon">
          {project.name?.charAt(0)?.toUpperCase() || "P"}
        </div>

        {canManage && (
          <div className="project-menu-wrapper">
            <button
              className="project-menu"
              type="button"
              aria-label="Project menu"
              aria-expanded={showActions}
              onClick={() => setShowActions((prev) => !prev)}
            >
              ⋮
            </button>

            {showActions && (
              <div className="project-actions">
                <button type="button" onClick={() => runAction(onEdit)}>
                  Edit
                </button>
                <button type="button" onClick={() => runAction(onMembers)}>
                  Manage members
                </button>
                <button
                  type="button"
                  className="danger"
                  onClick={() => runAction(onDelete)}
                >
                  Delete
                </button>
              </div>
            )}
          </div>
        )}
      </div>

      <h3>{project.name}</h3>

      <p>{project.description || "No description available."}</p>

      <div className="project-meta">
        <span className="badge">{project.status || "Planned"}</span>
        {project.dueDate && (
          <span className="badge">Due {formatDate(project.dueDate)}</span>
        )}
      </div>

      <div className="project-card-footer">
        <span>{project.taskCount ?? 0} Tasks</span>
        <button
          type="button"
          className="project-card-link"
          onClick={() => onOpen(project)}
        >
          View tasks →
        </button>
      </div>
    </article>
  );
}

export default ProjectCard;
