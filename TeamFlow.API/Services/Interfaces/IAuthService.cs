using TeamFlow.API.DTOs.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(
        RegisterDto dto);

    Task<AuthResponseDto> LoginAsync(
        LoginDto dto);

    Task<AuthResponseDto> RegisterFromInvitationAsync(
        RegisterFromInvitationDto dto);
}