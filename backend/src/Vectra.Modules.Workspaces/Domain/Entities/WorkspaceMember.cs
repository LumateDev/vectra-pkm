using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Workspaces.Domain.Entities
{
    public class WorkspaceMember : Entity
    {
        public Guid WorkspaceId { get; private set; }
        public Guid UserId { get; private set; }
        public WorkspaceMemberRole Role { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public DateTime? LastAccessedAt { get; private set; }

        public Workspace Workspace { get; private set; } = null!;

        protected WorkspaceMember() { }

        public WorkspaceMember(Guid workspaceId, Guid userId, WorkspaceMemberRole role)
        {
            WorkspaceId = workspaceId;
            UserId = userId;
            Role = role;
            JoinedAt = DateTime.UtcNow;
        }

        public void ChangeRole(WorkspaceMemberRole newRole)
        {
            if (Role == WorkspaceMemberRole.Owner && newRole != WorkspaceMemberRole.Owner)
                throw new InvalidOperationException("Cannot change owner role without transferring ownership");

            Role = newRole;
        }

        public void RecordAccess()
        {
            LastAccessedAt = DateTime.UtcNow;
        }
    }

}
