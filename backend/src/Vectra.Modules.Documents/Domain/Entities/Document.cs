using Vectra.Modules.Documents.Domain.Enums;
using Vectra.Modules.Documents.Domain.Events;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Documents.Domain.Entities
{
    public class Document : AuthoredEntity
    {
        public string Title { get; private set; } = null!;
        public Guid WorkspaceId { get; private set; }
        public Guid? FolderId { get; private set; } // null = документ в корне workspace
        public DocumentStatus Status { get; private set; }
        public int Version { get; private set; }
        public bool IsTemplate { get; private set; }
        public int Order { get; private set; }
        public bool IsDeleted { get; private set; }

        // Навигационные свойства
        private readonly List<Block> _blocks = new();
        private readonly List<DocumentTag> _tags = new();

        public IReadOnlyCollection<Block> Blocks => _blocks.AsReadOnly();
        public IReadOnlyCollection<DocumentTag> Tags => _tags.AsReadOnly();

        protected Document() { }

        public Document(string title, Guid workspaceId, Guid createdBy, Guid? folderId = null)
        {
            Title = title;
            WorkspaceId = workspaceId;
            FolderId = folderId;
            Status = DocumentStatus.Draft;
            Version = 1;
            IsTemplate = false;
            Order = 0;
            IsDeleted = false;
            CreatedBy = createdBy;

            AddDomainEvent(new DocumentCreatedEvent(Id, WorkspaceId, CreatedBy));
        }

        public void UpdateTitle(string title, Guid updatedBy)
        {
            var oldTitle = Title;
            Title = title;
            Version++;
            UpdateAuthor(updatedBy);

            AddDomainEvent(new DocumentTitleChangedEvent(Id, title, updatedBy));
        }

        public void ChangeStatus(DocumentStatus status, Guid changedBy)
        {
            var oldStatus = Status;
            Status = status;
            UpdateAuthor(changedBy);

            AddDomainEvent(new DocumentStatusChangedEvent(Id, oldStatus, status, changedBy));
        }

        public void MakeTemplate(Guid changedBy)
        {
            IsTemplate = true;
            UpdateAuthor(changedBy);

            AddDomainEvent(new DocumentMadeTemplateEvent(Id, changedBy));
        }

        public void Move(Guid? newFolderId, Guid movedBy)
        {
            var oldFolderId = FolderId;
            FolderId = newFolderId;
            UpdateAuthor(movedBy);

            AddDomainEvent(new DocumentMovedEvent(Id, oldFolderId, newFolderId, movedBy));
        }

        public void Delete(Guid deletedBy)
        {
            IsDeleted = true;
            Status = DocumentStatus.Deleted;
            UpdateAuthor(deletedBy);

            AddDomainEvent(new DocumentDeletedEvent(Id, deletedBy));
        }

        public void ChangeOrder(int newOrder, Guid changedBy)
        {
            Order = newOrder;
            UpdateAuthor(changedBy);
        }

        public void AddTag(Tag tag, Guid addedBy)
        {
            if (_tags.Any(dt => dt.TagId == tag.Id))
                return;

            _tags.Add(new DocumentTag(Id, tag.Id));
            UpdateAuthor(addedBy);

            AddDomainEvent(new DocumentTagAddedEvent(Id, tag.Id, addedBy));
        }

        public void RemoveTag(Guid tagId, Guid removedBy)
        {
            var documentTag = _tags.FirstOrDefault(dt => dt.TagId == tagId);
            if (documentTag == null)
                return;

            _tags.Remove(documentTag);
            UpdateAuthor(removedBy);

            AddDomainEvent(new DocumentTagRemovedEvent(Id, tagId, removedBy));
        }

        public Block AddBlock(BlockType type, string content, int order, Guid createdBy)
        {
            var block = new Block(Id, type, content, order, createdBy);
            _blocks.Add(block);
            Version++;
            UpdateAuthor(createdBy);

            return block;
        }

        public void RemoveBlock(Guid blockId, Guid removedBy)
        {
            var block = _blocks.FirstOrDefault(b => b.Id == blockId);
            if (block == null)
                return;

            _blocks.Remove(block);
            block.Delete(removedBy);
            Version++;
            UpdateAuthor(removedBy);
        }
    }

}
