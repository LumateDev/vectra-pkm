using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vectra.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlacklistedTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blacklisted_tokens",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Jti = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BlacklistedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blacklisted_tokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blacklisted_tokens_ExpiresAt",
                schema: "identity",
                table: "blacklisted_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_blacklisted_tokens_Jti",
                schema: "identity",
                table: "blacklisted_tokens",
                column: "Jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blacklisted_tokens_UserId",
                schema: "identity",
                table: "blacklisted_tokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blacklisted_tokens",
                schema: "identity");
        }
    }
}
