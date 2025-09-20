using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Documents.Domain.Events
{
    public record FolderCreatedEvent(Guid FolderId, string Name, Guid WorkspaceId, Guid? ParentId, Guid CreatedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record FolderRenamedEvent(Guid FolderId, string OldName, string NewName, Guid RenamedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record FolderMovedEvent(Guid FolderId, Guid? OldParentId, Guid? NewParentId, Guid MovedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record FolderDeletedEvent(Guid FolderId, Guid DeletedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
