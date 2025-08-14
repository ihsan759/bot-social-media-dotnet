using Xunit;
using bot_social_media.Utils;
using Microsoft.Extensions.Configuration;
using MicrosoftConfig = Microsoft.Extensions.Configuration.IConfiguration;


namespace bot_social_media.Tests.Utils
{
    public class JwtHandlingTests
    {
        private readonly JwtHandling _jwt;
        private readonly MicrosoftConfig _config;

        public JwtHandlingTests()
        {
            // Mock konfigurasi
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:AccessSecret", "RAR1AG0AbQB5AEEAYwBjAGUAcwBzAEsAZQB5AEYAbwByAFQAZQBzAHQAaQBuAGcATwBuAGwAeQA=" },
                { "Jwt:RefreshSecret", "RAR1AG0AbQB5AFIAZQBmAHIAZQBzAGgASwBlAHkARgBvAHIAVABlAHMAdABpAG4AZwBPAG4AbAB5AA==" },
                { "Jwt:AccessExpiryMinutes", "60" },
                { "Jwt:RefreshExpiryDays", "7" }
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _jwt = new JwtHandling(_config);
        }

        [Fact]
        public void GenAuthTokens_ShouldGenerateAccessAndRefreshTokens()
        {
            // Arrange
            var id = "123";
            var email = "test@example.com";
            var role = "USER";

            // Act
            var tokens = _jwt.GenAuthTokens(id, email, role);

            // Assert
            Assert.NotNull(tokens);
            Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
        }

        [Fact]
        public void ValidateToken_ShouldReturnClaimsPrincipal_WhenTokenValid()
        {
            // Arrange
            var id = "123";
            var email = "test@example.com";
            var role = "user";
            var tokens = _jwt.GenAuthTokens(id, email, role);

            // Act
            var principal = _jwt.ValidateToken(tokens.AccessToken, "Jwt:AccessSecret");

            // Assert
            Assert.NotNull(principal);
            Assert.Equal(id, principal.FindFirst("id")?.Value);
            Assert.Equal(email, principal.FindFirst("email")?.Value);
            Assert.Equal(role, principal.FindFirst("role")?.Value);
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_WhenTokenInvalid()
        {
            // Arrange
            var fakeToken = "this.is.an.invalid.token";

            // Act
            var principal = _jwt.ValidateToken(fakeToken, "Jwt:AccessSecret");

            // Assert
            Assert.Null(principal);
        }
    }
}
