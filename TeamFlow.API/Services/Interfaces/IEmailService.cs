public interface IEmailService
{
    Task SendInvitationEmailAsync(
        string email,
        string organizationName,
        string token);
}