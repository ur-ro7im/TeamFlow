import api from "../api/axiosClient";

export const getTaskFiles = async (taskId) => {
  const response = await api.get(`/File/task/${taskId}`);
  return response.data;
};

export const uploadTaskFile = async (taskId, file) => {
  const formData = new FormData();
  formData.append("file", file);

  const response = await api.post(`/File/task/${taskId}`, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return response.data;
};

export const deleteFile = async (fileId) => {
  await api.delete(`/File/${fileId}`);
};

// The endpoint needs the Authorization header, so the file is fetched as a
// blob and saved through a temporary link instead of a plain <a href>.
export const downloadFile = async (fileId, fileName) => {
  const response = await api.get(`/File/${fileId}/download`, {
    responseType: "blob",
  });

  const url = window.URL.createObjectURL(response.data);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
};
