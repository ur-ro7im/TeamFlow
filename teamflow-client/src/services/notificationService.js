import axiosClient from "../api/axiosClient";

export const getUnreadCount = async () => {
  const response = await axiosClient.get("/Notification/unread-count");

  return response.data;
};

export const getNotifications = async () => {
  const response = await axiosClient.get("/Notification");

  return response.data;
};

export const markAsRead = async (id) => {
  await axiosClient.patch(`/Notification/${id}/read`);
};

export const markAllAsRead = async () => {
  await axiosClient.patch("/Notification/read-all");
};
