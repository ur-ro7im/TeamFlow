using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.DTOs.Projects;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/members")]
[Authorize]
public class ProjectMembersController
    : ControllerBase
{
    private readonly IProjectMemberService
        _projectMemberService;

    public ProjectMembersController(
        IProjectMemberService projectMemberService)
    {
        _projectMemberService =
            projectMemberService;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> AddMember(
        int projectId,
        AddProjectMemberDto dto)
    {
        await _projectMemberService
            .AddMemberAsync(projectId, dto);

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(
    int projectId)
    {
        var members =
            await _projectMemberService
                .GetMembersAsync(projectId);

        return Ok(members);
    }

    [HttpDelete("{userId:int}")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> RemoveMember(
    int projectId,
    int userId)
    {
        await _projectMemberService
            .RemoveMemberAsync(
                projectId,
                userId);

        return NoContent();
    }
}