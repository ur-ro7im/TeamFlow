import { useEffect, useState } from "react";

import { useAuth } from "../../context/useAuth";
import {
  getMembers,
  getStorage,
  inviteMember,
  removeMember,
  updateMemberRole,
} from "../../services/organizationService";
import { getErrorMessage } from "../../utils/apiError";
import { formatBytes, formatDate } from "../../utils/format";
import { canManageOrganization, isOwner } from "../../utils/roles";
import "../tasks/tasks.css";
import "./members.css";

const ASSIGNABLE_ROLES = ["Admin", "Member"];

function MembersPage() {
  const { user } = useAuth();
  const canInvite = canManageOrganization(user?.role);
  const owner = isOwner(user?.role);

  const [state, setState] = useState({ loaded: false, items: [], error: "" });
  const [storage, setStorage] = useState(null);
  const [reloadKey, setReloadKey] = useState(0);

  const [actionError, setActionError] = useState("");
  const [busyUserId, setBusyUserId] = useState(null);

  const [inviteEmail, setInviteEmail] = useState("");
  const [inviteRole, setInviteRole] = useState("Member");
  const [inviting, setInviting] = useState(false);
  const [inviteError, setInviteError] = useState("");
  const [inviteSuccess, setInviteSuccess] = useState("");

  useEffect(() => {
    let cancelled = false;

    getMembers()
      .then((data) => {
        if (!cancelled) {
          setState({
            loaded: true,
            items: Array.isArray(data) ? data : [],
            error: "",
          });
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setState({
            loaded: true,
            items: [],
            error: getErrorMessage(err, "Failed to load members."),
          });
        }
      });

    return () => {
      cancelled = true;
    };
  }, [reloadKey]);

  useEffect(() => {
    if (!owner) {
      return undefined;
    }

    let cancelled = false;

    getStorage()
      .then((data) => {
        if (!cancelled) {
          setStorage(data);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setStorage(null);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [owner]);

  const reload = () => setReloadKey((key) => key + 1);

  const handleRoleChange = async (member, role) => {
    setBusyUserId(member.userId);
    setActionError("");

    try {
      await updateMemberRole(member.userId, role);
      reload();
    } catch (err) {
      setActionError(getErrorMessage(err, "Failed to change the role."));
    } finally {
      setBusyUserId(null);
    }
  };

  const handleRemove = async (member) => {
    const confirmed = window.confirm(
      `Remove ${member.fullName} from the organization?`,
    );

    if (!confirmed) {
      return;
    }

    setBusyUserId(member.userId);
    setActionError("");

    try {
      await removeMember(member.userId);
      reload();
    } catch (err) {
      setActionError(getErrorMessage(err, "Failed to remove the member."));
    } finally {
      setBusyUserId(null);
    }
  };

  const handleInvite = async (event) => {
    event.preventDefault();
    setInviteError("");
    setInviteSuccess("");

    const email = inviteEmail.trim();

    if (!email) {
      return;
    }

    setInviting(true);

    try {
      const invitation = await inviteMember({ email, role: inviteRole });
      setInviteEmail("");
      setInviteSuccess(
        `Invitation sent to ${invitation.email}. It expires on ${formatDate(invitation.expiresAt)}.`,
      );
    } catch (err) {
      setInviteError(getErrorMessage(err, "Failed to send the invitation."));
    } finally {
      setInviting(false);
    }
  };

  if (!state.loaded) {
    return <div className="page-loading">Loading members...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <h1>Members</h1>
        <p>People who belong to your organization.</p>
      </div>

      {state.error && (
        <div>
          <div className="error-message">{state.error}</div>
          <button className="btn-refresh" type="button" onClick={reload}>
            Retry
          </button>
        </div>
      )}

      {actionError && <div className="error-message">{actionError}</div>}

      <div className="members-layout">
        <div className="table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Role</th>
                <th>Joined</th>
                {owner && <th />}
              </tr>
            </thead>
            <tbody>
              {state.items.map((member) => {
                const isSelf = member.userId === user?.userId;
                const isMemberOwner = member.role === "Owner";

                return (
                  <tr key={member.userId}>
                    <td>
                      {member.fullName}
                      {isSelf ? " (you)" : ""}
                    </td>
                    <td>{member.email}</td>
                    <td>
                      {owner && !isMemberOwner ? (
                        <select
                          className="select-inline"
                          value={member.role}
                          disabled={busyUserId === member.userId}
                          onChange={(event) =>
                            handleRoleChange(member, event.target.value)
                          }
                          aria-label={`Role of ${member.fullName}`}
                        >
                          {ASSIGNABLE_ROLES.map((role) => (
                            <option key={role} value={role}>
                              {role}
                            </option>
                          ))}
                        </select>
                      ) : (
                        <span className="badge">{member.role}</span>
                      )}
                    </td>
                    <td>{formatDate(member.joinedAt)}</td>
                    {owner && (
                      <td>
                        {!isMemberOwner && (
                          <button
                            type="button"
                            className="link-danger"
                            disabled={busyUserId === member.userId}
                            onClick={() => handleRemove(member)}
                          >
                            Remove
                          </button>
                        )}
                      </td>
                    )}
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        <aside>
          {canInvite && (
            <div className="panel">
              <h2>Invite a member</h2>

              {inviteError && <div className="error-message">{inviteError}</div>}
              {inviteSuccess && (
                <div className="success-message">{inviteSuccess}</div>
              )}

              <form onSubmit={handleInvite}>
                <div className="form-group">
                  <label htmlFor="invite-email">Email</label>
                  <input
                    id="invite-email"
                    type="email"
                    value={inviteEmail}
                    onChange={(event) => setInviteEmail(event.target.value)}
                    maxLength={255}
                    required
                    disabled={inviting}
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="invite-role">Role</label>
                  <select
                    id="invite-role"
                    value={inviteRole}
                    onChange={(event) => setInviteRole(event.target.value)}
                    disabled={inviting}
                  >
                    {ASSIGNABLE_ROLES.map((role) => (
                      <option key={role} value={role}>
                        {role}
                      </option>
                    ))}
                  </select>
                </div>

                <button
                  type="submit"
                  className="primary-button"
                  disabled={inviting}
                >
                  {inviting ? "Sending..." : "Send invitation"}
                </button>
              </form>
            </div>
          )}

          {owner && storage && (
            <div className="panel">
              <h2>Storage</h2>
              <div
                className={`storage-bar${storage.usedPercentage >= 100 ? " is-full" : ""}`}
              >
                <div
                  style={{
                    width: `${Math.min(100, Math.max(0, storage.usedPercentage))}%`,
                  }}
                />
              </div>
              <p className="muted">
                {formatBytes(storage.usedStorage)} of{" "}
                {storage.maxStorage < 0
                  ? "unlimited"
                  : formatBytes(storage.maxStorage)}{" "}
                used
              </p>
            </div>
          )}
        </aside>
      </div>
    </div>
  );
}

export default MembersPage;
