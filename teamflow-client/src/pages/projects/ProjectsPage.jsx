import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import ProjectMembersModal from "../../components/projects/ProjectMembersModal";
import Modal from "../../components/ui/Modal";
import ProjectCard from "../../components/ui/ProjectCard";
import { useAuth } from "../../context/useAuth";
import {
  createProject,
  deleteProject,
  getProjects,
  updateProject,
} from "../../services/projectService";
import { getErrorMessage } from "../../utils/apiError";
import { canManageOrganization } from "../../utils/roles";
import ProjectForm from "./ProjectForm";
import "./project.css";

function ProjectsPage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const canManage = canManageOrganization(user?.role);

  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [formError, setFormError] = useState("");
  const [search, setSearch] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingProject, setEditingProject] = useState(null);
  const [membersProject, setMembersProject] = useState(null);

  useEffect(() => {
    let cancelled = false;

    getProjects()
      .then((data) => {
        if (cancelled) {
          return;
        }

        setProjects(Array.isArray(data) ? data : []);
        setError("");
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) {
          return;
        }

        setError(getErrorMessage(err, "Failed to load projects."));
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [reloadKey]);

  const retry = () => {
    setLoading(true);
    setReloadKey((key) => key + 1);
  };

  const closeForms = () => {
    setIsCreateModalOpen(false);
    setEditingProject(null);
    setFormError("");
  };

  const handleCreateProject = async (data) => {
    setSaving(true);
    setFormError("");

    try {
      const created = await createProject(data);
      setProjects((prev) => [created, ...prev]);
      closeForms();
    } catch (err) {
      setFormError(getErrorMessage(err, "Failed to create project."));
    } finally {
      setSaving(false);
    }
  };

  const handleUpdateProject = async (data) => {
    setSaving(true);
    setFormError("");

    try {
      const updated = await updateProject(editingProject.id, data);
      setProjects((prev) =>
        prev.map((project) => (project.id === updated.id ? updated : project)),
      );
      closeForms();
    } catch (err) {
      setFormError(getErrorMessage(err, "Failed to update project."));
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteProject = async (project) => {
    const confirmed = window.confirm(
      `Delete "${project.name}"? All of its tasks, comments and files will be removed. This cannot be undone.`,
    );

    if (!confirmed) {
      return;
    }

    setError("");

    try {
      await deleteProject(project.id);
      setProjects((prev) => prev.filter((p) => p.id !== project.id));
    } catch (err) {
      setError(getErrorMessage(err, "Failed to delete project."));
    }
  };

  const openTasks = (project) => navigate(`/tasks?project=${project.id}`);

  const filteredProjects = projects.filter((project) =>
    (project.name ?? "").toLowerCase().includes(search.toLowerCase()),
  );

  if (loading) {
    return <div className="page-loading">Loading projects...</div>;
  }

  return (
    <div>
      <div className="page-header-row">
        <div>
          <h1>Projects</h1>
          <p>Manage your team&apos;s projects.</p>
        </div>

        {canManage && (
          <button
            className="primary-button"
            type="button"
            onClick={() => setIsCreateModalOpen(true)}
          >
            + New Project
          </button>
        )}
      </div>

      {error && (
        <div>
          <div className="error-message">{error}</div>
          <button className="btn-refresh" type="button" onClick={retry}>
            Retry
          </button>
        </div>
      )}

      <div className="projects-toolbar">
        <input
          type="search"
          className="search-input"
          placeholder="Search projects..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
      </div>

      {filteredProjects.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">📁</div>
          <h2>No projects found</h2>
          <p>
            {search
              ? "Try a different search."
              : canManage
                ? "Create your first project to get started."
                : "You have not been added to any project yet. Ask an admin to add you."}
          </p>
          {!search && canManage && (
            <button
              className="primary-button"
              type="button"
              onClick={() => setIsCreateModalOpen(true)}
            >
              + Create Project
            </button>
          )}
        </div>
      ) : (
        <div className="projects-grid">
          {filteredProjects.map((project) => (
            <ProjectCard
              key={project.id}
              project={project}
              canManage={canManage}
              onOpen={openTasks}
              onEdit={(p) => {
                setFormError("");
                setEditingProject(p);
              }}
              onDelete={handleDeleteProject}
              onMembers={setMembersProject}
            />
          ))}
        </div>
      )}

      <Modal
        isOpen={isCreateModalOpen}
        onClose={closeForms}
        title="Create Project"
      >
        <ProjectForm
          onSubmit={handleCreateProject}
          loading={saving}
          error={formError}
        />
      </Modal>

      <Modal
        isOpen={Boolean(editingProject)}
        onClose={closeForms}
        title="Update Project"
      >
        {editingProject && (
          <ProjectForm
            key={editingProject.id}
            initialData={editingProject}
            onSubmit={handleUpdateProject}
            loading={saving}
            error={formError}
          />
        )}
      </Modal>

      {membersProject && (
        <ProjectMembersModal
          key={membersProject.id}
          project={membersProject}
          onClose={() => setMembersProject(null)}
        />
      )}
    </div>
  );
}

export default ProjectsPage;
