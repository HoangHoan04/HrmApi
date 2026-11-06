using HrmApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Đăng nhập trực tiếp tại HRM (bỏ cổng Auth tập trung):
    /// - Đảm bảo có role hệ thống ADMIN, EMPLOYEE
    /// - Đảm bảo tài khoản admin / admin123@ tồn tại, đang hoạt động, không bị khóa và được gán role ADMIN
    /// Idempotent — chạy lại không tạo trùng.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261106090000_SeedLocalAuthAdmin")]
    public partial class SeedLocalAuthAdmin : Migration
    {
        private const string SystemActor = "00000000-0000-0000-0000-000000000000";
        private const string AdminUserId = "00000000-0000-0000-0000-000000000001";

        // ASP.NET Identity PasswordHasher (V3) hash cho mật khẩu: admin123@
        private const string AdminPasswordHash =
            "AQAAAAIAAYagAAAAEITvLBC8mNjBkxCTF0525NRdxwsUM1pg8QOhHBF2lVmytM+LI3CDdbdW1OUcj77jhw==";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                -- 1. Role hệ thống
                INSERT INTO "RoleEntities" ("Id", "Code", "Name", "Description", "IsSystem", "IsActive", "CreatedBy", "CreatedAt", "IsDeleted")
                SELECT gen_random_uuid(), 'ADMIN', 'Quản trị hệ thống', 'Toàn quyền hệ thống', TRUE, TRUE, '{SystemActor}'::uuid, NOW(), FALSE
                WHERE NOT EXISTS (SELECT 1 FROM "RoleEntities" WHERE UPPER("Code") = 'ADMIN' AND "IsDeleted" = FALSE);

                INSERT INTO "RoleEntities" ("Id", "Code", "Name", "Description", "IsSystem", "IsActive", "CreatedBy", "CreatedAt", "IsDeleted")
                SELECT gen_random_uuid(), 'EMPLOYEE', 'Nhân viên', 'Vai trò mặc định của nhân viên', TRUE, TRUE, '{SystemActor}'::uuid, NOW(), FALSE
                WHERE NOT EXISTS (SELECT 1 FROM "RoleEntities" WHERE UPPER("Code") = 'EMPLOYEE' AND "IsDeleted" = FALSE);

                UPDATE "RoleEntities" SET "IsSystem" = TRUE, "IsActive" = TRUE
                WHERE UPPER("Code") IN ('ADMIN', 'EMPLOYEE') AND "IsDeleted" = FALSE;

                -- 2. Tài khoản admin
                INSERT INTO "UserEntities" (
                    "Id", "Username", "PasswordHash", "Type", "Email",
                    "IsActive", "IsLocked", "FailedLoginAttempts", "MustChangePassword", "TwoFactorEnabled",
                    "IsDeleted", "CreatedBy", "CreatedAt")
                SELECT
                    '{AdminUserId}'::uuid, 'admin', '{AdminPasswordHash}', 'ADMIN', 'admin@hrm.com',
                    TRUE, FALSE, 0, FALSE, FALSE,
                    FALSE, '{SystemActor}'::uuid, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM "UserEntities" WHERE LOWER("Username") = 'admin');

                UPDATE "UserEntities"
                SET "PasswordHash" = '{AdminPasswordHash}',
                    "Type" = 'ADMIN',
                    "IsActive" = TRUE,
                    "IsDeleted" = FALSE,
                    "IsLocked" = FALSE,
                    "LockedUntil" = NULL,
                    "FailedLoginAttempts" = 0,
                    "MustChangePassword" = FALSE,
                    "TwoFactorEnabled" = FALSE,
                    "UpdatedAt" = NOW()
                WHERE LOWER("Username") = 'admin';

                -- 3. Gán role ADMIN cho admin
                INSERT INTO "UserRoleEntities" ("Id", "UserId", "RoleId", "CreatedBy", "CreatedAt", "IsDeleted")
                SELECT gen_random_uuid(), u."Id", r."Id", '{SystemActor}'::uuid, NOW(), FALSE
                FROM "UserEntities" u
                CROSS JOIN LATERAL (
                    SELECT "Id" FROM "RoleEntities"
                    WHERE UPPER("Code") = 'ADMIN' AND "IsDeleted" = FALSE
                    ORDER BY "CreatedAt" LIMIT 1
                ) r
                WHERE LOWER(u."Username") = 'admin'
                  AND NOT EXISTS (
                      SELECT 1 FROM "UserRoleEntities" ur
                      WHERE ur."UserId" = u."Id" AND ur."RoleId" = r."Id" AND ur."IsDeleted" = FALSE
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dữ liệu seed — không xóa khi rollback để tránh mất tài khoản quản trị.
        }
    }
}
