using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultAdminUser : Migration
    {
        private const string AdminUserId = "00000000-0000-0000-0000-000000000001";
        private const string AdminPasswordHash = "AQAAAAIAAYagAAAAEO6DbdrBc3Oqab6pGCKnitV7IW/JnB7Og6GCraSPoR9moJ94Gvrpsz4SogeJqaiA9w==";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                INSERT INTO "UserEntities" (
                    "Id",
                    "Username",
                    "PasswordHash",
                    "Type",
                    "Email",
                    "IsActive",
                    "IsLocked",
                    "FailedLoginAttempts",
                    "MustChangePassword",
                    "IsDeleted",
                    "CreatedBy",
                    "CreatedAt"
                )
                SELECT
                    '{AdminUserId}'::uuid,
                    'admin',
                    '{AdminPasswordHash}',
                    'ADMIN',
                    'admin@hrm.com',
                    TRUE,
                    FALSE,
                    0,
                    FALSE,
                    FALSE,
                    '00000000-0000-0000-0000-000000000000'::uuid,
                    NOW() AT TIME ZONE 'UTC'
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "UserEntities"
                    WHERE LOWER("Username") = 'admin'
                      AND "IsDeleted" = FALSE
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                DELETE FROM "UserEntities"
                WHERE "Id" = '{AdminUserId}'::uuid;
                """);
        }
    }
}
