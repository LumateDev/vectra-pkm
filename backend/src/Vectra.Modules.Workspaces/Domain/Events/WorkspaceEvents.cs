using Vectra.Modules.Workspaces.Domain.Emuns;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Workspaces.Domain.Events
{
    public record WorkspaceCreatedEvent(Guid WorkspaceId, string Name, Guid OwnerId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record WorkspaceUpdatedEvent(Guid WorkspaceId, string NewName, Guid UpdatedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record WorkspaceArchivedEvent(Guid WorkspaceId, Guid ArchivedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record WorkspaceVisibilityChangedEvent(
        Guid WorkspaceId,
        WorkspaceVisibility OldVisibility,
        WorkspaceVisibility NewVisibility,
        Guid ChangedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record WorkspaceMemberAddedEvent(
        Guid WorkspaceId,
        Guid UserId,
        WorkspaceMemberRole Role,
        Guid AddedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record WorkspaceMemberRemovedEvent(Guid WorkspaceId, Guid UserId, Guid RemovedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public record WorkspaceMemberRoleChangedEvent(
        Guid WorkspaceId,
        Guid UserId,
        WorkspaceMemberRole OldRole,
        WorkspaceMemberRole NewRole,
        Guid ChangedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}