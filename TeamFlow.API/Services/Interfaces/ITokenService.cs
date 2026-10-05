using TeamFlow.API.Models;

namespace TeamFlow.API.Services.Interfaces;

public interface ITokenService
{
    string CreateToken(
        User user,
        int organizationId,
        string role
    );
}