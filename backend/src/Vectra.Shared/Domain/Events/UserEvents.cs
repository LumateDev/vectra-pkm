using Vectra.Shared.Domain.Primitives;

namespace Vectra.Shared.Domain.Events
{
    public record PasswordResetRequestedEvent(Guid UserId, string Email, string ResetToken) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
    public record UserCreatedEvent(Guid UserId, string Username) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

}
