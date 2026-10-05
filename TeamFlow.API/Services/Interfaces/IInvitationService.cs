using TeamFlow.API.DTOs.Organizations;

namespace TeamFlow.API.Services.Interfaces;

public interface IInvitationService
{
    Task<InvitationResponseDto> CreateInvitationAsync(
        InviteMemberDto dto);

    Task AcceptInvitationAsync(
        string token
        );
}