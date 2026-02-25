using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookingDomain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookingInfrastructure.Contracts.AuthService;
using BookingApplication.Abstractions.Contracts.AuthService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
public class AuthManager 
    (IConfiguration _configuration) : IAuthManager
{
    public string GenerateToken(Users user)
    {
        var signingCredentials = GetSigningCredentials();
        
        var claims = GetClaims(user);
        
        var tokenOption = GenerateTokenOptions(signingCredentials, claims);
        
        return new JwtSecurityTokenHandler().WriteToken(tokenOption);
    }

    private JwtSecurityToken GenerateTokenOptions
        (SigningCredentials signingCredentials, List<Claim> claims)
    {
        var jwtSettings = _configuration.GetSection("JwtConfig");
        
        var expiration = DateTime.UtcNow.AddSeconds(
            Convert.ToDouble(jwtSettings.GetSection("Lifetime").Value));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiration,
            signingCredentials: signingCredentials);
        
        return token;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var key = _configuration["JwtConfig:SecretKey"];
        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    private static List<Claim> GetClaims(Users user)
    {
        var userRoles = user.UserRoles
            .Select(ur => ur.Role)
            .ToList();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };
        
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role.Name)));
        
        return claims;
    }
}