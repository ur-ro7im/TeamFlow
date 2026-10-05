import api from "../api/axiosClient";

export const getTaskComments = async (taskId) => {
  const response = await api.get(`/Comment/task/${taskId}`);
  return response.data;
};

export const createComment = async (taskId, content) => {
  const response = await api.post(`/Comment/task/${taskId}`, { content });
  return response.data;
};

export const updateComment = async (commentId, content) => {
  await api.put(`/Comment/${commentId}`, { content });
};

export const deleteComment = async (commentId) => {
  await api.delete(`/Comment/${commentId}`);
};
