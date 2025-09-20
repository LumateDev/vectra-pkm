using Vectra.Shared.Domain.Primitives;

namespace Vectra.Shared.Domain.Events
{
    public record BlockContentUpdatedEvent(Guid BlockId, Guid DocumentId, string NewContent, Guid UpdatedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record BlockCreatedEvent(Guid BlockId, Guid DocumentId, Guid AuthorId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
