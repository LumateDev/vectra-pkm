using Vectra.Shared.Domain.Primitives;

namespace Vectra.Shared.Domain.Events
{
    public record DatabaseReadyEvent : IDomainEvent
    {
        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
