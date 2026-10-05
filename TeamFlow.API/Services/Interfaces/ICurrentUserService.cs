using System.Security.Claims;

namespace TeamFlow.API.Services.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }

    int OrganizationId { get; }

    string Role { get; }

    bool IsAuthenticated { get; }
}