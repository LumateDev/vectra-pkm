using Vectra.Modules.Documents.Domain.Enums;
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

    public record DocumentStatusChangedEvent(Guid DocumentId, DocumentStatus OldStatus, DocumentStatus NewStatus, Guid ChangedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record DocumentMadeTemplateEvent(Guid DocumentId, Guid ChangedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record DocumentTagAddedEvent(Guid DocumentId, Guid TagId, Guid AddedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record DocumentTagRemovedEvent(Guid DocumentId, Guid TagId, Guid RemovedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record DocumentMovedEvent(Guid DocumentId, Guid? OldFolderId, Guid? NewFolderId, Guid MovedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record DocumentDeletedEvent(Guid DocumentId, Guid DeletedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

}
