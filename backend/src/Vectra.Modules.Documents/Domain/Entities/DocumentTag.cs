using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Modules.Documents.Domain.Entities
{
    public class DocumentTag
    {
        public Guid DocumentId { get; private set; }
        public Guid TagId { get; private set; }
        public DateTime TaggedAt { get; private set; }

        public Document Document { get; private set; } = null!;
        public Tag Tag { get; private set; } = null!;

        protected DocumentTag() { }

        public DocumentTag(Guid documentId, Guid tagId)
        {
            DocumentId = documentId;
            TagId = tagId;
            TaggedAt = DateTime.UtcNow;
        }
    }
}
