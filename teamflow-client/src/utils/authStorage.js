const STORAGE_KEY = "teamflow_auth";

export const saveAuth = (auth) => {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(auth));
};

export const getAuth = () => {
  try {
    const stored = sessionStorage.getItem(STORAGE_KEY);

    return stored ? JSON.parse(stored) : null;
  } catch {
    sessionStorage.removeItem(STORAGE_KEY);
    return null;
  }
};

export const clearAuth = () => {
  sessionStorage.removeItem(STORAGE_KEY);
};

const PENDING_INVITE_KEY = "teamflow_pending_invite";

export const savePendingInvite = (token) => {
  sessionStorage.setItem(PENDING_INVITE_KEY, token);
};

export const getPendingInvite = () => sessionStorage.getItem(PENDING_INVITE_KEY);

export const clearPendingInvite = () => {
  sessionStorage.removeItem(PENDING_INVITE_KEY);
};
