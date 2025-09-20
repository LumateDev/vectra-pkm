using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Documents.Domain.Events
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
