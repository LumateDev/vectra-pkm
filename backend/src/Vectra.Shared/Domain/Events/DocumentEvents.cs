using Vectra.Shared.Domain.Primitives;

namespace Vectra.Shared.Domain.Events
{
    public record DocumentCreatedEvent(Guid DocumentId, Guid WorkspaceId, Guid AuthorId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record DocumentTitleChangedEvent(Guid DocumentId, string NewTitle, Guid ChangedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

}
