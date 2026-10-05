using System.Security.Claims;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor
            .HttpContext?
            .User
            .Identity?
            .IsAuthenticated
            ?? false;

    public int UserId
    {
        get
        {
            var value =
                _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirst(
                        ClaimTypes.NameIdentifier)
                    ?.Value;

            return int.Parse(value!);
        }
    }

    public int OrganizationId
    {
        get
        {
            var value =
                _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirst("OrganizationId")
                    ?.Value;

            return int.Parse(value!);
        }
    }

    public string Role
    {
        get
        {
            return _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(ClaimTypes.Role)
                ?.Value
                ?? string.Empty;
        }
    }
}