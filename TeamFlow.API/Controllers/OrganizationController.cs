using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.DTOs.Organizations;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(
        IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers()
    {
        var members =
            await _organizationService.GetMembersAsync();

        return Ok(members);
    }

    [HttpPost("members")]
    [Authorize(Policy = "AdminOrOwner")]
    public async Task<IActionResult> AddMember(
        AddMemberDto dto)
    {
        await _organizationService.AddMemberAsync(dto);

        return NoContent();
    }

    [HttpPut("members/{userId:int}/role")]
    [Authorize(Policy = "OwnerOnly")]
    public async Task<IActionResult> UpdateMemberRole(
        int userId,
        UpdateMemberRoleDto dto)
    {
        await _organizationService
            .UpdateMemberRoleAsync(userId, dto);

        return NoContent();
    }

    [HttpDelete("members/{userId:int}")]
    [Authorize(Policy = "OwnerOnly")]
    public async Task<IActionResult> RemoveMember(
        int userId)
    {
        await _organizationService
            .RemoveMemberAsync(userId);

        return NoContent();
    }

    [HttpGet("storage")]
    [Authorize(Policy = "OwnerOnly")]
    public async Task<IActionResult> GetStorage()
    {
        var result =
            await _organizationService.GetStorageAsync();

        return Ok(result);
    }
}
