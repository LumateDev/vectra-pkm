using Vectra.Modules.Identity.Domain.Enums;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Identity.Domain.Events
{
    public record PasswordResetRequestedEvent(Guid UserId, string Email, string ResetToken) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
    public record UserCreatedEvent(Guid UserId, string Username) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
    public record UserStatusChangedEvent(Guid UserId, UserStatus OldStatus, UserStatus NewStatus) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record UserRoleChangedEvent(Guid UserId, UserRole OldRole, UserRole NewRole) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record UserPasswordChangedEvent(Guid UserId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
    public record RefreshTokenRevokedEvent(Guid TokenId, Guid UserId, string? ReplacedByToken) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

}
