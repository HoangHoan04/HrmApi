using HrmApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Chuẩn hoá đăng nhập: tài khoản nhân viên dùng EMAIL CÔNG TY làm tên đăng nhập (trước đây là email cá nhân).
    /// - Username = Email = lower(trim(EmployeeEntities.CompanyEmail)) cho tài khoản đã gắn nhân viên.
    /// - Bỏ qua tài khoản quản trị / tài khoản chưa gắn nhân viên / nhân viên không có email công ty.
    /// - Bỏ qua nếu email công ty bị trùng với nhân viên khác hoặc đã là tên đăng nhập của tài khoản khác.
    /// - Mật khẩu giữ nguyên. Idempotent — chạy lại không thay đổi gì thêm.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261110090000_LoginByCompanyEmail")]
    public partial class LoginByCompanyEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH candidates AS (
                    SELECT u."Id" AS user_id, LOWER(TRIM(e."CompanyEmail")) AS login
                    FROM "UserEntities" u
                    JOIN "EmployeeEntities" e ON e."Id" = u."EmployeeId" AND e."IsDeleted" = FALSE
                    WHERE u."IsDeleted" = FALSE
                      AND COALESCE(u."Type", '') <> 'ADMIN'
                      AND NULLIF(TRIM(e."CompanyEmail"), '') IS NOT NULL
                ),
                unique_candidates AS (
                    -- email công ty không trùng giữa các tài khoản ứng viên
                    SELECT c.* FROM candidates c
                    WHERE (SELECT COUNT(*) FROM candidates c2 WHERE c2.login = c.login) = 1
                )
                UPDATE "UserEntities" u
                SET "Username" = uc.login,
                    "Email" = uc.login,
                    "UpdatedAt" = NOW()
                FROM unique_candidates uc
                WHERE u."Id" = uc.user_id
                  AND (LOWER(u."Username") <> uc.login OR LOWER(COALESCE(u."Email", '')) <> uc.login)
                  AND NOT EXISTS (
                      SELECT 1 FROM "UserEntities" o
                      WHERE o."Id" <> u."Id" AND o."IsDeleted" = FALSE
                        AND (LOWER(o."Username") = uc.login OR LOWER(COALESCE(o."Email", '')) = uc.login)
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Đổi lại tên đăng nhập = email cá nhân cho các tài khoản đang dùng email công ty.
            migrationBuilder.Sql("""
                UPDATE "UserEntities" u
                SET "Username" = LOWER(TRIM(e."Email")),
                    "Email" = LOWER(TRIM(e."Email")),
                    "UpdatedAt" = NOW()
                FROM "EmployeeEntities" e
                WHERE e."Id" = u."EmployeeId"
                  AND u."IsDeleted" = FALSE
                  AND NULLIF(TRIM(e."Email"), '') IS NOT NULL
                  AND LOWER(u."Username") = LOWER(TRIM(e."CompanyEmail"))
                  AND NOT EXISTS (
                      SELECT 1 FROM "UserEntities" o
                      WHERE o."Id" <> u."Id" AND o."IsDeleted" = FALSE
                        AND (LOWER(o."Username") = LOWER(TRIM(e."Email")) OR LOWER(COALESCE(o."Email", '')) = LOWER(TRIM(e."Email")))
                  );
                """);
        }
    }
}
