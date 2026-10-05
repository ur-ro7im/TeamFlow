import { useEffect, useState } from "react";

import Modal from "../ui/Modal";
import { getMembers } from "../../services/organizationService";
import {
  addProjectMember,
  getProjectMembers,
  removeProjectMember,
} from "../../services/projectService";
import { getErrorMessage } from "../../utils/apiError";

function ProjectMembersModal({ project, onClose }) {
  const [projectMembers, setProjectMembers] = useState([]);
  const [orgMembers, setOrgMembers] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState("");
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  const projectId = project?.id;

  useEffect(() => {
    if (!projectId) {
      return undefined;
    }

    let cancelled = false;

    Promise.all([getProjectMembers(projectId), getMembers()])
      .then(([inProject, inOrganization]) => {
        if (cancelled) {
          return;
        }

        setProjectMembers(Array.isArray(inProject) ? inProject : []);
        setOrgMembers(Array.isArray(inOrganization) ? inOrganization : []);
        setError("");
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) {
          return;
        }

        setError(getErrorMessage(err, "Failed to load members."));
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [projectId, reloadKey]);

  const availableMembers = orgMembers.filter(
    (member) => !projectMembers.some((pm) => pm.userId === member.userId),
  );

  const handleAdd = async () => {
    if (!selectedUserId) {
      return;
    }

    setBusy(true);
    setError("");

    try {
      await addProjectMember(projectId, Number(selectedUserId));
      setSelectedUserId("");
      setReloadKey((key) => key + 1);
    } catch (err) {
      setError(getErrorMessage(err, "Failed to add member."));
    } finally {
      setBusy(false);
    }
  };

  const handleRemove = async (userId) => {
    setBusy(true);
    setError("");

    try {
      await removeProjectMember(projectId, userId);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setError(getErrorMessage(err, "Failed to remove member."));
    } finally {
      setBusy(false);
    }
  };

  return (
    <Modal
      isOpen={Boolean(project)}
      onClose={onClose}
      title={project ? `Members — ${project.name}` : "Members"}
    >
      {error && <div className="error-message">{error}</div>}

      {loading ? (
        <p className="muted">Loading members...</p>
      ) : (
        <>
          <div className="inline-form">
            <select
              value={selectedUserId}
              onChange={(event) => setSelectedUserId(event.target.value)}
              disabled={busy || availableMembers.length === 0}
              aria-label="Organization member"
            >
              <option value="">
                {availableMembers.length === 0
                  ? "Everyone is already in this project"
                  : "Select a member..."}
              </option>
              {availableMembers.map((member) => (
                <option key={member.userId} value={member.userId}>
                  {member.fullName} ({member.email})
                </option>
              ))}
            </select>

            <button
              type="button"
              className="primary-button"
              onClick={handleAdd}
              disabled={busy || !selectedUserId}
            >
              Add
            </button>
          </div>

          {projectMembers.length === 0 ? (
            <p className="muted">No members in this project yet.</p>
          ) : (
            <ul className="simple-list">
              {projectMembers.map((member) => (
                <li key={member.userId}>
                  <div>
                    <strong>{member.fullName}</strong>
                    <span className="muted"> {member.email}</span>
                  </div>
                  <button
                    type="button"
                    className="link-danger"
                    onClick={() => handleRemove(member.userId)}
                    disabled={busy}
                  >
                    Remove
                  </button>
                </li>
              ))}
            </ul>
          )}
        </>
      )}
    </Modal>
  );
}

export default ProjectMembersModal;
