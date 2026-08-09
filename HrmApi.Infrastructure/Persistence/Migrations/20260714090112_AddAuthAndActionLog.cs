using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndActionLog : Migration
    {
        private const string AdminUserId = "00000000-0000-0000-0000-000000000001";
        // ASP.NET Identity PasswordHasher hash for password: admin123@
        private const string AdminPasswordHash =
            "AQAAAAIAAYagAAAAEITvLBC8mNjBkxCTF0525NRdxwsUM1pg8QOhHBF2lVmytM+LI3CDdbdW1OUcj77jhw==";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordOtp",
                table: "UserEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordOtpExpiresAt",
                table: "UserEntities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActionLogEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByCode = table.Column<string>(type: "text", nullable: false),
                    CreatedByName = table.Column<string>(type: "text", nullable: false),
                    CreatedNote = table.Column<string>(type: "text", nullable: true),
                    ActionType = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityName = table.Column<string>(type: "text", nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionLogEntities", x => x.Id);
                });

            // Default admin for fresh databases after migration.
            // Username: admin | Password: admin123@
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

            migrationBuilder.DropTable(
                name: "ActionLogEntities");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOtp",
                table: "UserEntities");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOtpExpiresAt",
                table: "UserEntities");
        }
    }
}
