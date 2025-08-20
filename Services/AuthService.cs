using System.Security.Cryptography;
using bot_social_media.Services;
using bot_social_media.Utils;
using BotSocialMedia.Models;
using BotSocialMedia.Repositories.Interfaces;
using static BCrypt.Net.BCrypt;

public class AuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly JwtHandling _jwtHandling;

    private readonly IConfiguration _config;
    private readonly EmailService _emailService;

    public AuthService(IAccountRepository accountRepo, JwtHandling jwtHandling, IConfiguration config, EmailService emailService)
    {
        _config = config;
        _emailService = emailService;
        _jwtHandling = jwtHandling;
        _accountRepository = accountRepo;
    }

    public async Task<AuthTokens> Register(RegisterDto dto)
    {
        var hashedPassword = HashPassword(dto.Password);
        if (await _accountRepository.Exists(x => x.Email == dto.Email))
            throw new HttpException("Email already exists", 400);
        if (await _accountRepository.Exists(x => x.Phone == dto.Phone))
            throw new HttpException("Phone number already exists", 400);
        if (await _accountRepository.Exists(x => x.Username == dto.Username))
            throw new HttpException("Username already exists", 400);
        var newAcc = new Accounts
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            Username = dto.Username,
            Password = hashedPassword,
            Phone = dto.Phone,
            Role = Role.USER,
            Status = Status.active,
            IsVerified = false,
            BirthDate = dto.BirthDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _accountRepository.Create(newAcc);
        var tokens = _jwtHandling.GenAuthTokens(created.Id.ToString(), created.Email, created.Role.ToString());

        await SendVerificationEmail(created.Id);

        await _accountRepository.Update(created.Id, e => e.RefreshToken = tokens.RefreshToken);

        return tokens;
    }

    public async Task<AuthTokens> Login(LoginDto dto)
    {
        Accounts? account = null;
        if (dto.Email != null)
        {
            account = await _accountRepository.GetByKey(x => x.Email == dto.Email);
        }
        else if (dto.Phone != null)
        {
            account = await _accountRepository.GetByKey(x => x.Phone == dto.Phone);
        }
        else if (dto.Username != null)
        {
            account = await _accountRepository.GetByKey(x => x.Username == dto.Username);
        }
        if (account == null || !Verify(dto.Password, account.Password))
            throw new HttpException("Invalid credentials", 401);

        if (account.Status != Status.active)
            throw new HttpException("Account not active", 403);

        var tokens = _jwtHandling.GenAuthTokens(account.Id.ToString(), account.Email, account.Role.ToString());
        await _accountRepository.Update(account.Id, e => e.RefreshToken = tokens.RefreshToken);
        return tokens;
    }

    public async Task<AuthTokens> RefreshToken(string refreshToken)
    {
        var account = await _accountRepository.GetByKey(x => x.RefreshToken == refreshToken);
        if (account == null) throw new HttpException("Forbidden", 403);

        var claims = _jwtHandling.ValidateToken(refreshToken, "Jwt:RefreshSecret");

        if (claims == null || claims.FindFirst("email")?.Value != account.Email)
            throw new HttpException("Invalid token", 401);

        var newToken = _jwtHandling.GenAuthTokens(account.Id.ToString(), account.Email, account.Role.ToString());
        await _accountRepository.Update(account.Id, e => e.RefreshToken = newToken.RefreshToken);
        return newToken;
    }

    public async Task ForgotPassword(string email)
    {
        var account = await _accountRepository.GetByKey(x => x.Email == email);
        if (account == null) throw new HttpException("User not found", 404);
        var bytes = RandomNumberGenerator.GetBytes(16); // 128-bit
        var resetToken = Convert.ToHexString(bytes);
        var resetTokenExpiry = DateTime.UtcNow.AddMinutes(5); // Token valid for 1 hour
        var resetUrl = $"{_config["App:BackendUrl"]}/api/auth/reset-password?token={resetToken}";
        var emailBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <title>Reset Password Akun QontrolBot</title>
                        </head>
                        <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px; color: #333;'>
                            <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 10px; padding: 30px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                                <h2 style='color: #4CAF50;'>Permintaan Reset Password</h2>
                                <p>Halo <strong>{account.Name}</strong>,</p>
                                <p>Untuk mereset password akun Anda, silakan gunakan token berikut:</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <div style='display: inline-block; padding: 12px 24px; font-size: 18px; font-weight: bold; color: #333; background-color: #f0f0f0; border-radius: 5px; border: 1px solid #ccc;'>
                                        {resetToken}
                                    </div>
                                </div>
                                <p style='font-size: 14px;'>
                                    Token ini hanya berlaku selama <strong>5 menit</strong> sejak email ini dikirim.<br/>
                                    Masukkan token tersebut pada halaman reset password di aplikasi QontrolBot.
                                </p>
                                <hr style='margin: 30px 0;'>
                                <p style='font-size: 12px; color: #888;'>
                                    Email ini dikirim secara otomatis oleh sistem QontrolBot. Mohon jangan membalas email ini.
                                </p>
                            </div>
                        </body>
                        </html>";


        await _accountRepository.Update(account.Id, acc =>
        {
            acc.ResetPasswordToken = resetToken;
            acc.ResetPasswordTokenExpiry = resetTokenExpiry;
            acc.UpdatedAt = DateTime.UtcNow;
        });
        try
        {
            await _emailService.SendEmailAsync(account.Email, "Konfirmasi Pendaftaran Akun QontrolBot", emailBody);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email send failed: {ex.Message}");
            throw;
        }
    }

    public async Task ResetPassword(ResetPasswordDto dto)
    {
        var account = await _accountRepository.GetByKey(a => a.Email == dto.Email && a.ResetPasswordToken == dto.Token);

        if (account == null)
            throw new HttpException("Invalid Token", 400);

        if (account.ResetPasswordTokenExpiry < DateTime.UtcNow)
            throw new HttpException("Token expired", 400);

        await _accountRepository.Update(account.Id, acc =>
        {
            acc.Password = HashPassword(dto.NewPassword);
            acc.ResetPasswordToken = null;
            acc.ResetPasswordTokenExpiry = null;
            acc.UpdatedAt = DateTime.UtcNow;
        });
    }

    public async Task Logout(string refreshToken)
    {
        var account = await _accountRepository.GetByKey(x => x.RefreshToken == refreshToken);
        if (account != null)
            await _accountRepository.Update(account.Id, e => e.RefreshToken = null);
    }

    private string GenerateVerificationToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<string> VerifyEmail(string token)
    {
        var account = await _accountRepository.GetByKey(a => a.VerificationToken == token);

        if (account == null)
            throw new HttpException("Invalid Token", 400);

        if (account.VerificationTokenExpiry < DateTime.UtcNow)
            throw new HttpException("Token expired", 400);

        if (account.IsVerified)
            return "Account already verified.";

        account.IsVerified = true;
        account.VerificationToken = null;
        account.UpdatedAt = DateTime.UtcNow;

        await _accountRepository.Update(account.Id, acc =>
        {
            acc.IsVerified = true;
            acc.VerificationToken = null;
            acc.UpdatedAt = DateTime.UtcNow;
        });

        return "Account successfully verified.";
    }

    public async Task SendVerificationEmail(Guid id)
    {
        var user = await _accountRepository.GetById(id) ?? throw new HttpException("User not found", 404);
        if (user.IsVerified)
            throw new HttpException("Account already verified", 400);
        var verificationToken = GenerateVerificationToken();
        var VerificationTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
        var verificationUrl = $"{_config["App:BackendUrl"]}/api/auth/verify-email?token={verificationToken}";
        var emailBody = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='UTF-8'>
                                <title>Verifikasi Akun QontrolBot</title>
                            </head>
                            <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px; color: #333;'>
                                <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 10px; padding: 30px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                                    <h2 style='color: #4CAF50;'>Konfirmasi Pendaftaran Akun</h2>
                                    <p>Halo <strong>{user.Name}</strong>,</p>
                                    <p>Terima kasih telah mendaftar di <strong>QontrolBot</strong>. Untuk mengaktifkan akun Anda, silakan klik tombol di bawah ini:</p>
                                    <div style='text-align: center; margin: 30px 0;'>
                                        <a href='{verificationUrl}' style='display: inline-block; padding: 12px 24px; font-size: 16px; color: white; background-color: #4CAF50; border-radius: 5px; text-decoration: none;'>Aktifkan Akun</a>
                                    </div>
                                    <p style='font-size: 14px;'>
                                        Tautan ini berlaku selama <strong>1 jam</strong> sejak email ini dikirim.
                                        Jika Anda tidak merasa mendaftar di QontrolBot, abaikan email ini.
                                    </p>
                                    <hr style='margin: 30px 0;'>
                                    <p style='font-size: 12px; color: #888;'>
                                        Email ini dikirim secara otomatis oleh sistem QontrolBot. Mohon jangan membalas email ini.
                                    </p>
                                </div>
                            </body>
                            </html>";


        await _accountRepository.Update(id, acc =>
        {
            acc.VerificationToken = verificationToken;
            acc.VerificationTokenExpiry = VerificationTokenExpiry;
            acc.UpdatedAt = DateTime.UtcNow;
        });
        try
        {
            await _emailService.SendEmailAsync(user.Email, "Konfirmasi Pendaftaran Akun QontrolBot", emailBody);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email send failed: {ex.Message}");
            throw;
        }
    }

}
