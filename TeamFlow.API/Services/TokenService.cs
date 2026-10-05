using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TeamFlow.API.Models;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(
        User user,
        int organizationId,
        string role)
    {
        var key = _configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");

        var claims = new List<Claim>
{
    new Claim(
        ClaimTypes.NameIdentifier,
        user.Id.ToString()
    ),

    new Claim(
        ClaimTypes.Name,
        user.FullName
    ),

    new Claim(
        ClaimTypes.Email,
        user.Email
    ),

    new Claim(
        "OrganizationId",
        organizationId.ToString()
    ),

    new Claim(
        ClaimTypes.Role,
        role
    )
};
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
             issuer: issuer,
             audience: audience,
             claims: claims,
             expires: expires,
             signingCredentials: credentials
         );

        return new JwtSecurityTokenHandler()
        .WriteToken(token);

    }
}