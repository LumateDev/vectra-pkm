using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Vectra.Modules.Documents.Domain.Enums;
using Vectra.Modules.Documents.Domain.Events;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Documents.Domain.Entities
{
    public class Block : AuthoredEntity
    {
        public Guid DocumentId { get; private set; }
        public BlockType Type { get; private set; }
        public string Content { get; private set; } = null!; // JSON
        public int Order { get; private set; }
        public Guid? ParentBlockId { get; private set; }

        public Document Document { get; private set; } = null!;

        protected Block() { }

        public Block(Guid documentId, BlockType type, string content, int order, Guid createdBy)
        {
            DocumentId = documentId;
            Type = type;
            Content = content;
            Order = order;
            CreatedBy = createdBy;

            AddDomainEvent(new BlockCreatedEvent(Id, DocumentId, CreatedBy));
        }

        public void UpdateContent(string content, Guid updatedBy)
        {
            Content = content;
            UpdateAuthor(updatedBy);

            AddDomainEvent(new BlockContentUpdatedEvent(Id, DocumentId, content, updatedBy));
        }

        public void ChangeOrder(int newOrder, Guid changedBy)
        {
            var oldOrder = Order;
            Order = newOrder;
            UpdateAuthor(changedBy);

            AddDomainEvent(new BlockOrderChangedEvent(Id, DocumentId, oldOrder, newOrder, changedBy));
        }

        public void Delete(Guid deletedBy)
        {
            AddDomainEvent(new BlockDeletedEvent(Id, DocumentId, deletedBy));
        }

        public void ChangeType(BlockType newType, Guid changedBy)
        {
            var oldType = Type;
            Type = newType;
            UpdateAuthor(changedBy);

            AddDomainEvent(new BlockTypeChangedEvent(Id, DocumentId, oldType, newType, changedBy));
        }
    }
}
