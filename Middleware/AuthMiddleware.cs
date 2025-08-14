using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _accessTokenSecret;

    public AuthMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _accessTokenSecret = config["Jwt:AccessSecret"]!;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            throw new HttpException("Unauthorized", 401);

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessTokenSecret)),
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(token, tokenValidationParameters, out _);
            context.Items["User"] = principal;
        }
        catch
        {
            throw new HttpException("Invalid or expired token", 401);
        }

        await _next(context);
    }
}
