export const canManageOrganization = (role) =>
  role === "Owner" || role === "Admin";

export const isOwner = (role) => role === "Owner";
