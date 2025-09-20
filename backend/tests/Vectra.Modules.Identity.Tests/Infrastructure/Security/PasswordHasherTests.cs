using System;
using FluentAssertions;
using Vectra.Modules.Identity.Infrastructure.Security;
using Xunit;

namespace Vectra.Modules.Identity.Tests.Infrastructure.Security
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _sut = new();
        private const string ValidPassword = "MySecurePassword123!";
        private const string WrongPassword = "WrongPassword123!";

        [Fact]
        public void HashPassword_WhenPasswordIsValid_ReturnsNonEmptyHash()
        {
            // Act
            var result = _sut.HashPassword(ValidPassword);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().NotBe(ValidPassword);
        }

        [Fact]
        public void HashPassword_WhenPasswordIsValid_ReturnsBcryptHashFormat()
        {
            // Act
            var result = _sut.HashPassword(ValidPassword);

            // Assert
            result.Should().StartWith("$2"); // BCrypt identifier
        }

        [Fact]
        public void HashPassword_WhenCalledTwiceWithSamePassword_ReturnsDifferentHashes()
        {
            // Act
            var hash1 = _sut.HashPassword(ValidPassword);
            var hash2 = _sut.HashPassword(ValidPassword);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void HashPassword_WhenPasswordIsInvalid_ThrowsArgumentException(string invalidPassword)
        {
            // Act & Assert
            var act = () => _sut.HashPassword(invalidPassword);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void VerifyPassword_WhenPasswordMatchesHash_ReturnsTrue()
        {
            // Arrange
            var hash = _sut.HashPassword(ValidPassword);

            // Act
            var result = _sut.VerifyPassword(ValidPassword, hash);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WhenPasswordDoesNotMatch_ReturnsFalse()
        {
            // Arrange
            var hash = _sut.HashPassword(ValidPassword);

            // Act
            var result = _sut.VerifyPassword(WrongPassword, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("", "$2a$12$hash")]
        [InlineData("password", "")]
        [InlineData("password", "invalid_hash_format")]
        public void VerifyPassword_WhenInputIsInvalid_ReturnsFalse(string password, string hash)
        {
            // Act
            var result = _sut.VerifyPassword(password, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void NeedsRehash_WhenHashHasLowWorkFactor_ReturnsTrue()
        {
            // Arrange
            var lowWorkFactorHash = "$2a$04$rXZ2zrZ5zrZ5zrZ5zrZ5zO";

            // Act
            var result = _sut.NeedsRehash(lowWorkFactorHash);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void NeedsRehash_WhenHashHasAdequateWorkFactor_ReturnsFalse()
        {
            // Arrange
            var adequateHash = _sut.HashPassword(ValidPassword);

            // Act
            var result = _sut.NeedsRehash(adequateHash);

            // Assert
            result.Should().BeFalse();
        }
    }
}
