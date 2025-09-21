using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vectra.Modules.Identity.Domain.Entities;

namespace Vectra.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class BlacklistedTokenConfiguration : IEntityTypeConfiguration<BlacklistedToken>
    {
        public void Configure(EntityTypeBuilder<BlacklistedToken> builder)
        {
            builder.ToTable("blacklisted_tokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Jti)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Reason)
                .HasMaxLength(200);

            // Индексы
            builder.HasIndex(x => x.Jti)
                .IsUnique();

            builder.HasIndex(x => x.ExpiresAt);
            builder.HasIndex(x => x.UserId);

            // Игнорируем доменные события
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
