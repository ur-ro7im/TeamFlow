using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.DTOs.Organizations;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvitationController : ControllerBase
{
    private readonly IInvitationService _invitationService;

    public InvitationController(
        IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> CreateInvitation(
        InviteMemberDto dto)
    {
        var invitation =
            await _invitationService
                .CreateInvitationAsync(dto);

        return Ok(invitation);
    }

    [HttpPost("accept")]
    public async Task<IActionResult> AcceptInvitation(
        AcceptInvitationDto dto)
    {
        await _invitationService
            .AcceptInvitationAsync(dto.Token);

        return NoContent();
    }
}