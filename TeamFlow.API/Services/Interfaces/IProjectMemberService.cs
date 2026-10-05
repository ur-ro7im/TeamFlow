using TeamFlow.API.DTOs.Projects;

namespace TeamFlow.API.Services.Interfaces;

public interface IProjectMemberService
{
    Task AddMemberAsync(
        int projectId,
        AddProjectMemberDto dto);

    Task<List<ProjectMemberResponseDto>>
        GetMembersAsync(int projectId);

    Task RemoveMemberAsync(
        int projectId,
        int userId);
}