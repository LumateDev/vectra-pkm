using FluentAssertions;
using Vectra.Modules.Identity.Infrastructure.Security;


namespace Vectra.Modules.Identity.Tests.Infrastructure.Security
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _sut = new();
        private const string ValidPassword = "MySecurePassword123!";
        private const string WrongPassword = "WrongPassword123!";

        [Fact]
        public void HashPassword_WithValidPassword_ReturnsNonEmptyBcryptHash()
        {
            // Act
            var result = _sut.HashPassword(ValidPassword);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().NotBe(ValidPassword);
            result.Should().StartWith("$2"); // BCrypt format
            result.Length.Should().BeGreaterThan(50); // BCrypt hash length
        }

        [Fact]
        public void HashPassword_WhenCalledTwice_ReturnsDifferentHashesDueToSalt()
        {
            // Act
            var hash1 = _sut.HashPassword(ValidPassword);
            var hash2 = _sut.HashPassword(ValidPassword);

            // Assert
            hash1.Should().NotBe(hash2);
            hash1.Should().StartWith("$2");
            hash2.Should().StartWith("$2");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void HashPassword_WithEmptyOrWhitespacePassword_ThrowsArgumentException(string invalidPassword)
        {
            // Act & Assert
            var act = () => _sut.HashPassword(invalidPassword);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Password cannot be empty*")
                .WithParameterName("password");
        }

        [Fact]
        public void HashPassword_WithNullPassword_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => _sut.HashPassword(null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("password");
        }
        [Fact]
        public void VerifyPassword_WithNullPassword_ReturnsFalse()
        {
            // Arrange
            var hash = _sut.HashPassword(ValidPassword);

            // Act
            var result = _sut.VerifyPassword(null!, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_WithNullHash_ReturnsFalse()
        {
            // Act
            var result = _sut.VerifyPassword(ValidPassword, null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            var hash = _sut.HashPassword(ValidPassword);

            // Act
            var result = _sut.VerifyPassword(ValidPassword, hash);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
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
        [InlineData("password", "invalid_hash")]
        [InlineData("password", "too_short")]
        public void VerifyPassword_WithInvalidInput_ReturnsFalse(string password, string hash)
        {
            // Act
            var result = _sut.VerifyPassword(password, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("$2a$04$invalidhash", true)]    // Low work factor
        [InlineData("$2a$08$invalidhash", true)]    // Medium work factor
        [InlineData("invalid_hash", true)]          // Invalid format
        [InlineData("", true)]                      // Empty hash
        public void NeedsRehash_WithVariousHashes_ReturnsExpectedResult(string passwordHash, bool expectedResult)
        {
            // Act
            var result = _sut.NeedsRehash(passwordHash);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void NeedsRehash_WithAdequateHash_ReturnsFalse()
        {
            // Arrange
            var adequateHash = _sut.HashPassword(ValidPassword);

            // Act
            var result = _sut.NeedsRehash(adequateHash);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void FullCycle_HashAndVerify_WorksCorrectly()
        {
            // Arrange
            const string testPassword = "Test@123";

            // Act
            var hash = _sut.HashPassword(testPassword);
            var verificationResult = _sut.VerifyPassword(testPassword, hash);
            var needsRehash = _sut.NeedsRehash(hash);

            // Assert
            verificationResult.Should().BeTrue();
            needsRehash.Should().BeFalse();
            hash.Should().StartWith("$2");
        }
    }
}