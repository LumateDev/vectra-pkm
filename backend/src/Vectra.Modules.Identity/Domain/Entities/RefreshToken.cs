using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vectra.Modules.Identity.Domain.Events;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Identity.Domain.Entities
{
    public class RefreshToken : Entity
    {
        public string Token { get; private set; } = null!;
        public Guid UserId { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string? CreatedByIp { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? ReplacedByToken { get; private set; }

        public User User { get; private set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        protected RefreshToken() { }

        public RefreshToken(string token, Guid userId, int daysToExpire = 7, string? createdByIp = null)
        {
            Token = token;
            UserId = userId;
            ExpiresAt = DateTime.UtcNow.AddDays(daysToExpire);
            CreatedAt = DateTime.UtcNow;
            CreatedByIp = createdByIp;
        }

        public void Revoke(string? revokedByIp = null, string? replacedByToken = null)
        {
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = revokedByIp;
            ReplacedByToken = replacedByToken;

            AddDomainEvent(new RefreshTokenRevokedEvent(Id, UserId, replacedByToken));
        }
    }
}
