using AuthentificationService.Service.Services;
using FluentAssertions;

namespace AuthenticationService.UnitTests
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _passwordHasher;

        public PasswordHasherTests()
        {
            _passwordHasher = new PasswordHasher();
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedPassword()
        {
            // Arrange
            var password = "MonSuperMotDePasse123!";

            // Act
            var hash = _passwordHasher.HashPassword(password);

            // Assert
            hash.Should().NotBeNullOrEmpty();
            hash.Should().NotBe(password);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var password = "MonSuperMotDePasse123!";
            var hash = _passwordHasher.HashPassword(password);

            // Act
            var result = _passwordHasher.VerifyPassword(password, hash);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_IncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            var password = "MonSuperMotDePasse123!";
            var wrongPassword = "MauvaisMotDePasse";
            var hash = _passwordHasher.HashPassword(password);

            // Act
            var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

            // Assert
            result.Should().BeFalse();
        }
    }
}