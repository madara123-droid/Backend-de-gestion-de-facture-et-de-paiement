using AuthentificationSerice.Core.Entitie;
using AuthentificationSerice.Core.Entitie;
using AuthentificationService.Service.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationService.UnitTests
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly User _testUser;

        public TokenServiceTests()
        {
            // Créer une configuration IN-MEMORY réelle (plus simple et plus fiable)
            var inMemorySettings = new Dictionary<string, string>
            {
                {"JwtSettings:Secret", "MaSuperCleSecretePourJwtDoitEtreLongue32CaracteresMin"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"JwtSettings:AccessTokenExpirationMinutes", "15"}
            };

            // Utiliser ConfigurationBuilder pour créer une vraie configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Créer le service avec la configuration réelle
            _tokenService = new TokenService(configuration);

            // Créer un utilisateur de test
            _testUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Role = "Admin"
            };
        }

        [Fact]
        public void GenerateAccessToken_ShouldReturnValidToken()
        {
            // Act
            var token = _tokenService.GenerateAccessToken(_testUser);

            // Assert
            token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Should().NotBeNull();
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == _testUser.Email);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == _testUser.Role);
            jwtToken.Claims.Should().Contain(c => c.Type == "userId" && c.Value == _testUser.Id.ToString());
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnValidToken()
        {
            // Act
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Assert
            refreshToken.Should().NotBeNullOrEmpty();
            refreshToken.Length.Should().BeGreaterThan(32);
        }

        [Fact]
        public void ValidateRefreshToken_ValidToken_ShouldReturnTrue()
        {
            // Arrange
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Act
            var result = _tokenService.ValidateRefreshToken(refreshToken);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateRefreshToken_InvalidToken_ShouldReturnFalse()
        {
            // Act
            var result = _tokenService.ValidateRefreshToken("invalid");

            // Assert
            result.Should().BeFalse();
        }
    }
}