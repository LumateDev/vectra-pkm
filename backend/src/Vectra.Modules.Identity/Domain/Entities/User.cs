using Vectra.Modules.Identity.Domain.Enums;
using Vectra.Shared.Domain.Events;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Identity.Domain.Entities
{
    public class User : AuditableEntity
    {
        public string Email { get; private set; } = null!;
        public string Username { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public UserStatus Status { get; private set; }
        public UserRole Role { get; private set; }

        // Навигационные свойства
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        protected User() { }

        public User(string email, string username, string passwordHash)
        {
            Email = email.ToLowerInvariant();
            Username = username;
            PasswordHash = passwordHash;
            Status = UserStatus.Active;
            Role = UserRole.User;

            AddDomainEvent(new UserCreatedEvent(Id, Username));
        }

        public void ChangeStatus(UserStatus newStatus)
        {
            var oldStatus = Status;
            Status = newStatus;
            UpdateTimestamp();

            AddDomainEvent(new UserStatusChangedEvent(Id, oldStatus, newStatus));
        }

        public void ChangeRole(UserRole newRole)
        {
            var oldRole = Role;
            Role = newRole;
            UpdateTimestamp();

            AddDomainEvent(new UserRoleChangedEvent(Id, oldRole, newRole));
        }

        public void UpdatePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdateTimestamp();

            AddDomainEvent(new UserPasswordChangedEvent(Id));
        }

        public void RequestPasswordReset(string resetToken)
        {
            AddDomainEvent(new PasswordResetRequestedEvent(Id, Email, resetToken));
        }
    }
}
