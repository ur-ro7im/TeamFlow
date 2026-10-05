import { useEffect, useRef, useState } from "react";

import {
  deleteFile,
  downloadFile,
  getTaskFiles,
  uploadTaskFile,
} from "../../services/fileService";
import { getErrorMessage } from "../../utils/apiError";
import { formatBytes, formatDateTime } from "../../utils/format";
import { canManageOrganization } from "../../utils/roles";

const MAX_FILE_SIZE = 10 * 1024 * 1024;
const ACCEPTED_TYPES = ".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.txt";

function TaskFiles({ taskId, user }) {
  const [files, setFiles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [reloadKey, setReloadKey] = useState(0);
  const [busy, setBusy] = useState(false);
  const inputRef = useRef(null);

  useEffect(() => {
    let cancelled = false;

    getTaskFiles(taskId)
      .then((data) => {
        if (cancelled) {
          return;
        }

        setFiles(Array.isArray(data) ? data : []);
        setError("");
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) {
          return;
        }

        setError(getErrorMessage(err, "Failed to load attachments."));
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [taskId, reloadKey]);

  const reload = () => setReloadKey((key) => key + 1);

  const canDelete = (file) =>
    file.uploadedByUserId === user?.userId || canManageOrganization(user?.role);

  const handleUpload = async (event) => {
    const file = event.target.files?.[0];

    // Allow selecting the same file again afterwards.
    event.target.value = "";

    if (!file) {
      return;
    }

    if (file.size > MAX_FILE_SIZE) {
      setError("File size cannot exceed 10 MB.");
      return;
    }

    setBusy(true);
    setError("");

    try {
      await uploadTaskFile(taskId, file);
      reload();
    } catch (err) {
      setError(getErrorMessage(err, "Failed to upload the file."));
    } finally {
      setBusy(false);
    }
  };

  const handleDownload = async (file) => {
    setError("");

    try {
      await downloadFile(file.id, file.fileName);
    } catch (err) {
      setError(getErrorMessage(err, "Failed to download the file."));
    }
  };

  const handleDelete = async (file) => {
    if (!window.confirm(`Delete "${file.fileName}"?`)) {
      return;
    }

    setBusy(true);
    setError("");

    try {
      await deleteFile(file.id);
      reload();
    } catch (err) {
      setError(getErrorMessage(err, "Failed to delete the file."));
    } finally {
      setBusy(false);
    }
  };

  return (
    <section className="detail-section">
      <div className="detail-section-header">
        <h3>Attachments</h3>

        <div>
          <input
            ref={inputRef}
            type="file"
            accept={ACCEPTED_TYPES}
            onChange={handleUpload}
            hidden
          />
          <button
            type="button"
            className="secondary-button"
            onClick={() => inputRef.current?.click()}
            disabled={busy}
          >
            {busy ? "Working..." : "Upload file"}
          </button>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      {loading ? (
        <p className="muted">Loading attachments...</p>
      ) : files.length === 0 ? (
        <p className="muted">No attachments.</p>
      ) : (
        <ul className="simple-list">
          {files.map((file) => (
            <li key={file.id}>
              <div>
                <strong>{file.fileName}</strong>
                <div className="muted">
                  {formatBytes(file.size)} · {file.uploadedByUserName} ·{" "}
                  {formatDateTime(file.createdAt)}
                </div>
              </div>

              <div className="row-actions">
                <button
                  type="button"
                  className="link-action"
                  onClick={() => handleDownload(file)}
                >
                  Download
                </button>
                {canDelete(file) && (
                  <button
                    type="button"
                    className="link-danger"
                    onClick={() => handleDelete(file)}
                    disabled={busy}
                  >
                    Delete
                  </button>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

export default TaskFiles;
