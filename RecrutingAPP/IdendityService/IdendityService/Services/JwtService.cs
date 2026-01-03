using IdendityService.Interfaces;
using IdendityService.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdendityService.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _cfg;
        private readonly byte[] _key;
        public JwtService(IConfiguration cfg)
        {
            _cfg = cfg;
            _key = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]);
        }

        public string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles, out DateTime expiresAt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(_key);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
        };

            foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

            expiresAt = DateTime.UtcNow.AddMinutes(30); // access token lifetime
            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return tokenHandler.WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        public ClaimsPrincipal? ValidatePrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _cfg["Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = _cfg["Jwt:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateLifetime = false // we want to get principal from expired token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is JwtSecurityToken jwt && jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }
            }
            catch
            {
                // invalid token
            }

            return null;
        }
    }
}
