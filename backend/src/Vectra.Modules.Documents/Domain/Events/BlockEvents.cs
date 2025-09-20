using Vectra.Modules.Documents.Domain.Enums;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Documents.Domain.Events
{
    public record BlockCreatedEvent(Guid BlockId, Guid DocumentId, Guid AuthorId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record BlockContentUpdatedEvent(Guid BlockId, Guid DocumentId, string NewContent, Guid UpdatedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record BlockOrderChangedEvent(Guid BlockId, Guid DocumentId, int OldOrder, int NewOrder, Guid ChangedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record BlockDeletedEvent(Guid BlockId, Guid DocumentId, Guid DeletedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record BlockTypeChangedEvent(Guid BlockId, Guid DocumentId, BlockType OldType, BlockType NewType, Guid ChangedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
