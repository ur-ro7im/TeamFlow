import { useEffect, useState } from "react";

import {
  createComment,
  deleteComment,
  getTaskComments,
  updateComment,
} from "../../services/commentService";
import { getErrorMessage } from "../../utils/apiError";
import { formatDateTime } from "../../utils/format";
import { canManageOrganization } from "../../utils/roles";

function TaskComments({ taskId, user }) {
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  const [newComment, setNewComment] = useState("");
  const [editingId, setEditingId] = useState(null);
  const [editingText, setEditingText] = useState("");
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    let cancelled = false;

    getTaskComments(taskId)
      .then((data) => {
        if (cancelled) {
          return;
        }

        setComments(Array.isArray(data) ? data : []);
        setError("");
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) {
          return;
        }

        setError(getErrorMessage(err, "Failed to load comments."));
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [taskId, reloadKey]);

  const reload = () => setReloadKey((key) => key + 1);

  const canModify = (comment) =>
    comment.userId === user?.userId || canManageOrganization(user?.role);

  const handleAdd = async (event) => {
    event.preventDefault();

    const content = newComment.trim();

    if (!content) {
      return;
    }

    setBusy(true);
    setError("");

    try {
      await createComment(taskId, content);
      setNewComment("");
      reload();
    } catch (err) {
      setError(getErrorMessage(err, "Failed to add the comment."));
    } finally {
      setBusy(false);
    }
  };

  const handleSaveEdit = async (commentId) => {
    const content = editingText.trim();

    if (!content) {
      return;
    }

    setBusy(true);
    setError("");

    try {
      await updateComment(commentId, content);
      setEditingId(null);
      setEditingText("");
      reload();
    } catch (err) {
      setError(getErrorMessage(err, "Failed to update the comment."));
    } finally {
      setBusy(false);
    }
  };

  const handleDelete = async (commentId) => {
    if (!window.confirm("Delete this comment?")) {
      return;
    }

    setBusy(true);
    setError("");

    try {
      await deleteComment(commentId);
      reload();
    } catch (err) {
      setError(getErrorMessage(err, "Failed to delete the comment."));
    } finally {
      setBusy(false);
    }
  };

  return (
    <section className="detail-section">
      <h3>Comments</h3>

      {error && <div className="error-message">{error}</div>}

      {loading ? (
        <p className="muted">Loading comments...</p>
      ) : comments.length === 0 ? (
        <p className="muted">No comments yet.</p>
      ) : (
        <ul className="comment-list">
          {comments.map((comment) => (
            <li key={comment.id} className="comment">
              <div className="comment-header">
                <strong>{comment.userName}</strong>
                <span className="muted">
                  {formatDateTime(comment.createdAt)}
                  {comment.updatedAt ? " (edited)" : ""}
                </span>
              </div>

              {editingId === comment.id ? (
                <div>
                  <textarea
                    className="comment-input"
                    rows="3"
                    value={editingText}
                    maxLength={3000}
                    onChange={(event) => setEditingText(event.target.value)}
                  />
                  <div className="comment-actions">
                    <button
                      type="button"
                      className="link-action"
                      onClick={() => handleSaveEdit(comment.id)}
                      disabled={busy || !editingText.trim()}
                    >
                      Save
                    </button>
                    <button
                      type="button"
                      className="link-action"
                      onClick={() => setEditingId(null)}
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                <>
                  <p className="comment-body">{comment.content}</p>
                  {canModify(comment) && (
                    <div className="comment-actions">
                      <button
                        type="button"
                        className="link-action"
                        onClick={() => {
                          setEditingId(comment.id);
                          setEditingText(comment.content);
                        }}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="link-danger"
                        onClick={() => handleDelete(comment.id)}
                        disabled={busy}
                      >
                        Delete
                      </button>
                    </div>
                  )}
                </>
              )}
            </li>
          ))}
        </ul>
      )}

      <form onSubmit={handleAdd} className="comment-form">
        <textarea
          className="comment-input"
          rows="3"
          placeholder="Write a comment..."
          value={newComment}
          maxLength={3000}
          onChange={(event) => setNewComment(event.target.value)}
          disabled={busy}
        />
        <button
          type="submit"
          className="primary-button"
          disabled={busy || !newComment.trim()}
        >
          {busy ? "Saving..." : "Add comment"}
        </button>
      </form>
    </section>
  );
}

export default TaskComments;
