using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace bot_social_media.Utils
{
    public class JwtHandling
    {
        private readonly IConfiguration _config;
        public JwtHandling(IConfiguration config)
        {
            _config = config;
        }

        public AuthTokens GenAuthTokens(string id, string email, string role)
        {
            var accessToken = SignToken(id, email, role, "Jwt:AccessSecret", int.Parse(_config["Jwt:AccessExpiryMinutes"]!));
            var refreshToken = SignToken(id, email, role, "Jwt:RefreshSecret", int.Parse(_config["Jwt:RefreshExpiryDays"]!) * 1440);

            return new AuthTokens { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        private string SignToken(string id, string email, string role, string secretKeyName, int expiryMinutes)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config[secretKeyName]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim("id", id),
            new Claim("email", email),
            new Claim("role", role.ToLower())
        };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token, string secretKeyName)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // nonaktifkan claim mapping default

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config[secretKeyName]!));
            var handler = new JwtSecurityTokenHandler();

            try
            {
                return handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = key,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
