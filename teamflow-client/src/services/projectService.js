import api from "../api/axiosClient";

export const getProjects = async () => {
  const response = await api.get("/projects");
  return response.data;
};

export const createProject = async (data) => {
  const response = await api.post("/projects", data);
  return response.data;
};

export const updateProject = async (id, data) => {
  const response = await api.put(`/projects/${id}`, data);
  return response.data;
};

export const deleteProject = async (id) => {
  await api.delete(`/projects/${id}`);
};

export const getProjectMembers = async (projectId) => {
  const response = await api.get(`/projects/${projectId}/members`);
  return response.data;
};

export const addProjectMember = async (projectId, userId) => {
  await api.post(`/projects/${projectId}/members`, { userId });
};

export const removeProjectMember = async (projectId, userId) => {
  await api.delete(`/projects/${projectId}/members/${userId}`);
};
