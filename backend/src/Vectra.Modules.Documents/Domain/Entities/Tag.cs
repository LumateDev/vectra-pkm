using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Documents.Domain.Entities
{
    public class Tag : Entity
    {
        public string Name { get; private set; } = null!;
        public string Color { get; private set; } = null!;
        public Guid WorkspaceId { get; private set; }

        public ICollection<DocumentTag> Documents { get; private set; } = new List<DocumentTag>();

        protected Tag() { }

        public Tag(string name, Guid workspaceId, string? color = null)
        {
            Name = name.ToLowerInvariant().Trim();
            WorkspaceId = workspaceId;
            Color = color ?? GenerateRandomColor();
        }

        public void UpdateName(string name)
        {
            Name = name.ToLowerInvariant().Trim();
        }

        public void UpdateColor(string color)
        {
            Color = color;
        }

        private static string GenerateRandomColor()
        {
            var colors = new[]
            {
            "#FF6B6B", // Red
            "#4ECDC4", // Teal
            "#45B7D1", // Blue
            "#96CEB4", // Green
            "#FECA57", // Yellow
            "#DDA0DD", // Purple
            "#F8B500", // Orange
            "#6C5CE7", // Violet
            "#A8E6CF", // Mint
            "#FFD93D"  // Gold
        };
            return colors[Random.Shared.Next(colors.Length)];
        }
    }
}
