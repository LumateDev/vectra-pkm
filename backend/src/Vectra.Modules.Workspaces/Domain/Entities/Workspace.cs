using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vectra.Modules.Workspaces.Domain.Emuns;
using Vectra.Modules.Workspaces.Domain.Events;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Workspaces.Domain.Entities
{
    public class Workspace : AuthoredEntity
    {
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public WorkspaceVisibility Visibility { get; private set; }
        public bool IsArchived { get; private set; }

        // Навигационные свойства
        private readonly List<WorkspaceMember> _members = new();
        public IReadOnlyCollection<WorkspaceMember> Members => _members.AsReadOnly();

        protected Workspace() { }

        public Workspace(string name, Guid ownerId, string? description = null)
        {
            Name = name;
            Description = description;
            Visibility = WorkspaceVisibility.Private;
            IsArchived = false;
            CreatedBy = ownerId;

            // Создатель автоматически становится владельцем
            var ownerMember = new WorkspaceMember(Id, ownerId, WorkspaceMemberRole.Owner);
            _members.Add(ownerMember);

            AddDomainEvent(new WorkspaceCreatedEvent(Id, Name, ownerId));
        }

        public void Update(string name, string? description, Guid updatedBy)
        {
            Name = name;
            Description = description;
            UpdateAuthor(updatedBy);

            AddDomainEvent(new WorkspaceUpdatedEvent(Id, name, updatedBy));
        }

        public void Archive(Guid archivedBy)
        {
            IsArchived = true;
            UpdateAuthor(archivedBy);

            AddDomainEvent(new WorkspaceArchivedEvent(Id, archivedBy));
        }

        public void ChangeVisibility(WorkspaceVisibility visibility, Guid changedBy)
        {
            var oldVisibility = Visibility;
            Visibility = visibility;
            UpdateAuthor(changedBy);

            AddDomainEvent(new WorkspaceVisibilityChangedEvent(Id, oldVisibility, visibility, changedBy));
        }

        public void AddMember(Guid userId, WorkspaceMemberRole role, Guid addedBy)
        {
            if (_members.Any(m => m.UserId == userId))
                throw new InvalidOperationException("User is already a member of this workspace");

            var member = new WorkspaceMember(Id, userId, role);
            _members.Add(member);

            AddDomainEvent(new WorkspaceMemberAddedEvent(Id, userId, role, addedBy));
        }

        public void RemoveMember(Guid userId, Guid removedBy)
        {
            var member = _members.FirstOrDefault(m => m.UserId == userId);
            if (member == null)
                throw new InvalidOperationException("User is not a member of this workspace");

            if (member.Role == WorkspaceMemberRole.Owner)
                throw new InvalidOperationException("Cannot remove workspace owner");

            _members.Remove(member);

            AddDomainEvent(new WorkspaceMemberRemovedEvent(Id, userId, removedBy));
        }
    }

}
