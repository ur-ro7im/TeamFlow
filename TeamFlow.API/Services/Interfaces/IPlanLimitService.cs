namespace TeamFlow.API.Services.Interfaces;

public interface IPlanLimitService
{
    Task<bool> CanCreateProjectAsync(int organizationId);

    Task<bool> CanAddMemberAsync(int organizationId);

    Task<bool> CanUseStorageAsync(
        int organizationId,
        long additionalBytes);
}