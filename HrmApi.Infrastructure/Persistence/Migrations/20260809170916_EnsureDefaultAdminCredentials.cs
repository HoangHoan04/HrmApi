using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Sync default admin credentials on existing databases.
    /// Fresh installs already get admin from AddAuthAndActionLog.
    /// Username: admin | Password: admin123@
    /// </remarks>
    public partial class EnsureDefaultAdminCredentials : Migration
    {
        private const string AdminUserId = "00000000-0000-0000-0000-000000000001";
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
            // Keep admin account; do not delete on rollback.
        }
    }
}
