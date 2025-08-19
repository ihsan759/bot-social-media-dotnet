using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AuthService _authService;

    public AuthController(AuthService authService, IConfiguration config)
    {

        _config = config;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var tokens = await _authService.Register(dto);
        return Ok(new
        {
            status = 200,
            message = "Registration successful",
            data = tokens
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var tokens = await _authService.Login(dto);
        Response.Cookies.Append("jwt", tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in production
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshExpiryDays"]!))
        });
        return Ok(new
        {
            status = 200,
            message = "Login successful",
            data = tokens.AccessToken
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["jwt"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new HttpException("Refresh token not found in cookies", 401);
        }
        var tokens = await _authService.RefreshToken(refreshToken);
        Response.Cookies.Append("jwt", tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in production
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshExpiryDays"]!))
        });
        return Ok(new
        {
            status = 200,
            message = "Token refreshed",
            data = tokens.AccessToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["jwt"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new HttpException("Refresh token not found in cookies", 401);
        }

        await _authService.Logout(refreshToken);

        // Hapus cookie
        Response.Cookies.Delete("jwt");

        return Ok(new
        {
            status = 200,
            message = "Logged out and cookie deleted"
        });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var result = await _authService.VerifyEmail(token);
        return Ok(new
        {
            status = 200,
            message = result
        });
    }

    [HttpPost("resend-Verification")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "user")]
    public async Task<IActionResult> ResendVerification()
    {

        // pastikan klaim "id" memang ada di token JWT Anda.
        var id = HttpContext.User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(id))
            throw new HttpException("User ID not found in token", StatusCodes.Status401Unauthorized);

        await _authService.SendVerificationEmail(Guid.Parse(id));

        return Ok(new
        {
            status = 200,
            message = "Verification email sent."
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPassword(dto.Email);
        return Ok(new
        {
            status = 200,
            message = "Password reset email sent"
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        await _authService.ResetPassword(dto);
        return Ok(new
        {
            status = 200,
            message = "Password has been reset successfully"
        });
    }


}
