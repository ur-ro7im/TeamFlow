import Modal from "../ui/Modal";
import { formatDate, formatDateTime } from "../../utils/format";
import { STATUS_LABELS } from "../../utils/taskConstants";
import TaskComments from "./TaskComments";
import TaskFiles from "./TaskFiles";

function TaskDetailModal({ task, user, onClose }) {
  return (
    <Modal
      isOpen={Boolean(task)}
      onClose={onClose}
      title={task?.title ?? "Task"}
      wide
    >
      {task && (
        <div className="task-detail">
          <div className="task-detail-meta">
            <span className={`badge badge-${task.status}`}>
              {STATUS_LABELS[task.status] ?? task.status}
            </span>
            <span className={`badge badge-${task.priority}`}>{task.priority}</span>
            <span className="muted">Due: {formatDate(task.dueDate)}</span>
            <span className="muted">
              Assignee: {task.assignedToUserName || "Unassigned"}
            </span>
            <span className="muted">
              Created by {task.createdByUserName} on {formatDateTime(task.createdAt)}
            </span>
          </div>

          <section className="detail-section">
            <h3>Description</h3>
            <p className="comment-body">
              {task.description || "No description provided."}
            </p>
          </section>

          <TaskFiles taskId={task.id} user={user} />
          <TaskComments taskId={task.id} user={user} />

          <p className="muted">
            The task owner is notified when someone comments or attaches a file.
          </p>
        </div>
      )}
    </Modal>
  );
}

export default TaskDetailModal;
