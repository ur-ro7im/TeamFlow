import api from "../api/axiosClient";

export const getMembers = async () => {
  const response = await api.get("/Organization/members");
  return response.data;
};

export const updateMemberRole = async (userId, role) => {
  await api.put(`/Organization/members/${userId}/role`, { role });
};

export const removeMember = async (userId) => {
  await api.delete(`/Organization/members/${userId}`);
};

export const getStorage = async () => {
  const response = await api.get("/Organization/storage");
  return response.data;
};

export const inviteMember = async (data) => {
  const response = await api.post("/Invitation", data);
  return response.data;
};

export const acceptInvitation = async (token) => {
  await api.post("/Invitation/accept", { token });
};
