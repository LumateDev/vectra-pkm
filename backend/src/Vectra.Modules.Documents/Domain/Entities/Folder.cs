using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Vectra.Modules.Documents.Domain.Events;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Documents.Domain.Entities
{
    public class Folder : AuthoredEntity
    {
        public string Name { get; private set; } = null!;
        public Guid WorkspaceId { get; private set; }
        public Guid? ParentId { get; private set; } // null = корневая папка
        public string? Icon { get; private set; }
        public string? Color { get; private set; }
        public int Order { get; private set; }
        public bool IsDeleted { get; private set; }

        // Навигационные свойства
        private readonly List<Folder> _subfolders = new();
        private readonly List<Document> _documents = new();

        public IReadOnlyCollection<Folder> Subfolders => _subfolders.AsReadOnly();
        public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

        protected Folder() { }

        public Folder(string name, Guid workspaceId, Guid createdBy, Guid? parentId = null)
        {
            Name = name;
            WorkspaceId = workspaceId;
            ParentId = parentId;
            CreatedBy = createdBy;
            Order = 0;
            IsDeleted = false;

            AddDomainEvent(new FolderCreatedEvent(Id, Name, WorkspaceId, ParentId, CreatedBy));
        }

        public void Rename(string newName, Guid renamedBy)
        {
            var oldName = Name;
            Name = newName;
            UpdateAuthor(renamedBy);

            AddDomainEvent(new FolderRenamedEvent(Id, oldName, newName, renamedBy));
        }

        public void Move(Guid? newParentId, Guid movedBy)
        {
            var oldParentId = ParentId;
            ParentId = newParentId;
            UpdateAuthor(movedBy);

            AddDomainEvent(new FolderMovedEvent(Id, oldParentId, newParentId, movedBy));
        }

        public void Delete(Guid deletedBy)
        {
            IsDeleted = true;
            UpdateAuthor(deletedBy);

            // Рекурсивно удаляем подпапки
            foreach (var subfolder in _subfolders)
            {
                subfolder.Delete(deletedBy);
            }

            // Удаляем документы
            foreach (var document in _documents)
            {
                document.Delete(deletedBy);
            }

            AddDomainEvent(new FolderDeletedEvent(Id, deletedBy));
        }

        public void ChangeOrder(int newOrder, Guid changedBy)
        {
            Order = newOrder;
            UpdateAuthor(changedBy);
        }
    }
}
