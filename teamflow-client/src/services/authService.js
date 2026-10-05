import axiosClient from "../api/axiosClient";

export const login = async (data) => {
  const response = await axiosClient.post("/Auth/login", data);
  return response.data;
};

export const register = async (data) => {
  const response = await axiosClient.post("/Auth/register", data);
  return response.data;
};

export const registerFromInvitation = async (data) => {
  const response = await axiosClient.post("/Auth/register-from-invitation", data);
  return response.data;
};
