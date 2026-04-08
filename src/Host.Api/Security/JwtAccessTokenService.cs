using Identity.Application.Configuration;
using Identity.Application.Services;
using Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Host.Api.Security;

public sealed class JwtAccessTokenService(IOptions<JwtTokenOptions> options) : IJwtAccessTokenService
{
    private readonly JwtTokenOptions _options = options.Value;

    public AccessTokenResult CreateToken(AccessTokenRequest request)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes <= 0 ? 15 : _options.AccessTokenMinutes);
        var tokenId = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(SecurityClaimTypes.Subject, request.UserId.ToString()),
            new(SecurityClaimTypes.UserId, request.UserId.ToString()),
            new(SecurityClaimTypes.UserCode, request.UserCode),
            new(SecurityClaimTypes.Username, request.UserCode),
            new(SecurityClaimTypes.NameIdentifier, request.UserId.ToString()),
            new(SecurityClaimTypes.Name, request.UserCode),
            new(SecurityClaimTypes.SessionId, request.SessionId.ToString()),
            new(JwtRegisteredClaimNames.Jti, tokenId)
        };

        if (request.CompanyId.HasValue)
        {
            claims.Add(new Claim(SecurityClaimTypes.CompanyId, request.CompanyId.Value.ToString()));
        }

        foreach (var role in request.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(SecurityClaimTypes.RoleClaim, role));
            claims.Add(new Claim(SecurityClaimTypes.Role, role));
        }

        foreach (var permission in request.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(SecurityClaimTypes.Permission, permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new AccessTokenResult(token, expiresAt, tokenId);
    }
}
