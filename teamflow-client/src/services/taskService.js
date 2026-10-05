import api from "../api/axiosClient";

const cleanParams = (params) =>
  Object.fromEntries(
    Object.entries(params).filter(
      ([, value]) => value !== "" && value !== null && value !== undefined,
    ),
  );

export const getProjectTasks = async (projectId, params = {}) => {
  const response = await api.get(`/Task/project/${projectId}`, {
    params: cleanParams(params),
  });
  return response.data;
};

export const getTask = async (taskId) => {
  const response = await api.get(`/Task/${taskId}`);
  return response.data;
};

export const createTask = async (projectId, data) => {
  const response = await api.post(`/Task/project/${projectId}`, data);
  return response.data;
};

export const updateTask = async (taskId, data) => {
  await api.put(`/Task/${taskId}`, data);
};

export const updateTaskStatus = async (taskId, status) => {
  await api.patch(`/Task/${taskId}/status`, { status });
};

export const deleteTask = async (taskId) => {
  await api.delete(`/Task/${taskId}`);
};
