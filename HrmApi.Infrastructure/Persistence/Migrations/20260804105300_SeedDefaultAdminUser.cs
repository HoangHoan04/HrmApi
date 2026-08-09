using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Kept for migration history compatibility.
    /// Default admin is seeded in AddAuthAndActionLog (admin / admin123@).
    /// This migration only ensures the same credentials on databases that already
    /// ran AddAuthAndActionLog before the seed was added there.
    /// </remarks>
    public partial class SeedDefaultAdminUser : Migration
    {
        private const string AdminUserId = "00000000-0000-0000-0000-000000000001";
        // ASP.NET Identity PasswordHasher hash for password: admin123@
        private const string AdminPasswordHash =
            "AQAAAAIAAYagAAAAEITvLBC8mNjBkxCTF0525NRdxwsUM1pg8QOhHBF2lVmytM+LI3CDdbdW1OUcj77jhw==";

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

                UPDATE "UserEntities"
                SET
                    "PasswordHash" = '{AdminPasswordHash}',
                    "IsActive" = TRUE,
                    "IsLocked" = FALSE,
                    "LockedUntil" = NULL,
                    "FailedLoginAttempts" = 0,
                    "MustChangePassword" = FALSE,
                    "UpdatedAt" = NOW() AT TIME ZONE 'UTC'
                WHERE LOWER("Username") = 'admin'
                  AND "IsDeleted" = FALSE;
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
