using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services.Email;

/// <summary>
/// Development stand-in for a real mail provider. It writes the invitation link to the
/// log at Debug level (enabled in appsettings.Development.json only) so invitations can be
/// tested locally. Replace it with an SMTP / transactional-email implementation of
/// IEmailService before going to production.
/// </summary>
public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;
    private readonly IConfiguration _configuration;

    public FakeEmailService(
        ILogger<FakeEmailService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task SendInvitationEmailAsync(
        string email,
        string organizationName,
        string token)
    {
        var clientBaseUrl =
            (_configuration["Client:BaseUrl"] ?? "http://localhost:5173")
            .TrimEnd('/');

        _logger.LogInformation(
            "Invitation created for {Email} in organization {Organization}.",
            email,
            organizationName);

        // The token is a credential, so it is only logged at Debug level.
        _logger.LogDebug(
            "Invitation link for {Email}: {Link}",
            email,
            $"{clientBaseUrl}/invite?token={Uri.EscapeDataString(token)}");

        return Task.CompletedTask;
    }
}
