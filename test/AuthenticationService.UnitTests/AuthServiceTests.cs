using AuthentificationSerice.Core.DTOs;
using AuthentificationSerice.Core.Entitie;
using AuthentificationSerice.Core.Interfaces.Repositories;
using AuthentificationService.Service.Interfaces;
using AuthentificationService.Service.Services;
using AuthentificationSerice.Core.Entitie;
using AuthentificationSerice.Core.Interfaces.Repositories;
using AuthentificationService.Service.Interfaces;
using AuthentificationService.Service.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using static AuthentificationSerice.Core.DTOs.LoginDto;

namespace AuthenticationService.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnLoginResponse()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                PasswordHash = "hashedPassword",
                Role = "Comptable",
                IsActive = true
            };

            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "password"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(true);

            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");

            _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("access-token");
            result.RefreshToken.Should().Be("refresh-token");
            result.Role.Should().Be("Comptable");

            _userRepositoryMock.Verify(x => x.addRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ShouldReturnNull()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "unknown@test.com",
                Password = "password"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(x => x.addRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ShouldReturnNull()
        {
            // Arrange
            var user = new User
            {
                Email = "test@test.com",
                PasswordHash = "hashedPassword",
                IsActive = false
            };

            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "password"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_NewEmail_ShouldReturnTrue()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "new@test.com",
                Password = "password",
                Role = "Comptable"
            };

            _userRepositoryMock.Setup(x => x.EmailExistAsync(request.Email))
                .ReturnsAsync(false);

            _passwordHasherMock.Setup(x => x.HashPassword(request.Password))
                .Returns("hashedPassword");

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u => u.Email == request.Email)), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ExistingEmail_ShouldReturnFalse()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@test.com",
                Password = "password"
            };

            _userRepositoryMock.Setup(x => x.EmailExistAsync(request.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }
    }
}