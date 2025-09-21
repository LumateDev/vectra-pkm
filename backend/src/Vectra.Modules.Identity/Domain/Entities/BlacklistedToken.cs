using System;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Identity.Domain.Entities
{
    public class BlacklistedToken : Entity
    {
        public string Jti { get; private set; } = null!; // JWT ID
        public DateTime ExpiresAt { get; private set; }
        public DateTime BlacklistedAt { get; private set; }
        public Guid UserId { get; private set; }
        public string? Reason { get; private set; }

        protected BlacklistedToken() { }

        public BlacklistedToken(string jti, DateTime expiresAt, Guid userId, string? reason = null)
        {
            Jti = jti;
            ExpiresAt = expiresAt;
            UserId = userId;
            BlacklistedAt = DateTime.UtcNow;
            Reason = reason;
        }
    }
}
