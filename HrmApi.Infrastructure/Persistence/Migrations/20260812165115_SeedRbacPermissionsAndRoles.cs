using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Seeds permission catalog + system roles (ADMIN/HR/MANAGER/EMPLOYEE)
    /// and assigns default admin user to ADMIN.
    /// DataScope is stored on RolePermission for Phase B filtering.
    /// Idempotent upsert by Code.
    /// </remarks>
    public partial class SeedRbacPermissionsAndRoles : Migration
    {
        private const string ActorId = "00000000-0000-0000-0000-000000000000";
        private const string AdminUserId = "00000000-0000-0000-0000-000000000001";
        private const string RoleAdminId = "10000000-0000-0000-0000-000000000001";
        private const string RoleHrId = "10000000-0000-0000-0000-000000000002";
        private const string RoleManagerId = "10000000-0000-0000-0000-000000000003";
        private const string RoleEmployeeId = "10000000-0000-0000-0000-000000000004";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildSeedSql());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                DELETE FROM "UserRoleEntities"
                WHERE "Id" = '40000000-0000-0000-0000-000000000001'::uuid;

                DELETE FROM "RolePermissionEntities"
                WHERE "Id"::text LIKE '30000000-0000-0000-%';

                DELETE FROM "RoleEntities"
                WHERE "Id" IN (
                    '{RoleAdminId}'::uuid,
                    '{RoleHrId}'::uuid,
                    '{RoleManagerId}'::uuid,
                    '{RoleEmployeeId}'::uuid
                );

                DELETE FROM "PermissionEntities"
                WHERE "Id"::text LIKE '20000000-0000-0000-0000-%';
                """);
        }

        private static string BuildSeedSql()
        {
            // Code|Name|Module|Action|IsScopable|FixedIdSuffix
            string[] permissions =
            [
                "ORG_VIEW|Xem tổ chức|ORGANIZATION|VIEW|true|000000000001",
                "ORG_MANAGE|Quản lý tổ chức|ORGANIZATION|MANAGE|true|000000000002",
                "EMPLOYEE_VIEW|Xem nhân viên|EMPLOYEE|VIEW|true|000000000003",
                "EMPLOYEE_CREATE|Tạo nhân viên|EMPLOYEE|CREATE|true|000000000004",
                "EMPLOYEE_UPDATE|Cập nhật nhân viên|EMPLOYEE|UPDATE|true|000000000005",
                "EMPLOYEE_DELETE|Xóa nhân viên|EMPLOYEE|DELETE|true|000000000006",
                "EMPLOYEE_MANAGE|Quản lý nhân viên|EMPLOYEE|MANAGE|true|000000000007",
                "TIMEKEEPING_VIEW|Xem chấm công|TIMEKEEPING|VIEW|true|000000000008",
                "TIMEKEEPING_ADJUST|Điều chỉnh chấm công|TIMEKEEPING|ADJUST|true|000000000009",
                "TIMEKEEPING_MANAGE|Quản lý chấm công|TIMEKEEPING|MANAGE|true|000000000010",
                "SHIFT_VIEW|Xem ca làm việc|SHIFT|VIEW|true|000000000011",
                "SHIFT_CREATE|Tạo ca làm việc|SHIFT|CREATE|true|000000000012",
                "SHIFT_UPDATE|Cập nhật ca làm việc|SHIFT|UPDATE|true|000000000013",
                "SHIFT_MANAGE|Quản lý ca làm việc|SHIFT|MANAGE|true|000000000014",
                "LEAVE_VIEW|Xem nghỉ phép|LEAVE|VIEW|true|000000000015",
                "LEAVE_CREATE|Tạo đơn nghỉ phép|LEAVE|CREATE|true|000000000016",
                "LEAVE_APPROVE|Duyệt nghỉ phép|LEAVE|APPROVE|true|000000000017",
                "LEAVE_MANAGE|Quản lý nghỉ phép|LEAVE|MANAGE|true|000000000018",
                "ATTENDANCE_COMPLAINT_VIEW|Xem khiếu nại chấm công|ATTENDANCE_COMPLAINT|VIEW|true|000000000019",
                "ATTENDANCE_COMPLAINT_CREATE|Tạo khiếu nại chấm công|ATTENDANCE_COMPLAINT|CREATE|true|000000000020",
                "ATTENDANCE_COMPLAINT_REVIEW|Duyệt khiếu nại chấm công|ATTENDANCE_COMPLAINT|REVIEW|true|000000000021",
                "CONTRACT_VIEW|Xem hợp đồng|CONTRACT|VIEW|true|000000000022",
                "CONTRACT_CREATE|Tạo hợp đồng|CONTRACT|CREATE|true|000000000023",
                "CONTRACT_UPDATE|Cập nhật hợp đồng|CONTRACT|UPDATE|true|000000000024",
                "CONTRACT_MANAGE|Quản lý hợp đồng|CONTRACT|MANAGE|true|000000000025",
                "PAYROLL_VIEW|Xem lương|PAYROLL|VIEW|true|000000000026",
                "PAYROLL_CREATE|Tạo bảng lương|PAYROLL|CREATE|true|000000000027",
                "PAYROLL_MANAGE|Quản lý lương|PAYROLL|MANAGE|true|000000000028",
                "ROLE_VIEW|Xem vai trò|ROLE|VIEW|true|000000000029",
                "ROLE_MANAGE|Quản lý vai trò|ROLE|MANAGE|true|000000000030",
                "USER_VIEW|Xem người dùng|USER|VIEW|true|000000000031",
                "USER_MANAGE|Quản lý người dùng|USER|MANAGE|true|000000000032",
                "ACTION_LOG_VIEW|Xem nhật ký thao tác|ACTION_LOG|VIEW|false|000000000033",
                "MOBILE_ACCESS|Truy cập ứng dụng di động|MOBILE|ACCESS|false|000000000034",
            ];

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-- Permissions");
            foreach (string row in permissions)
            {
                string[] p = row.Split('|');
                string code = p[0], name = p[1].Replace("'", "''"), module = p[2], action = p[3];
                string scopable = p[4] == "true" ? "TRUE" : "FALSE";
                string id = $"20000000-0000-0000-0000-{p[5]}";
                sb.AppendLine($"""
                    INSERT INTO "PermissionEntities" ("Id","Code","Name","Module","Action","Description","IsScopable","IsSystem","CreatedBy","CreatedAt","IsDeleted","Version")
                    SELECT '{id}'::uuid, '{code}', '{name}', '{module}', '{action}', NULL, {scopable}, TRUE, '{ActorId}'::uuid, NOW() AT TIME ZONE 'UTC', FALSE, NULL
                    WHERE NOT EXISTS (SELECT 1 FROM "PermissionEntities" WHERE "Code" = '{code}' AND "IsDeleted" = FALSE);
                    UPDATE "PermissionEntities"
                    SET "Name" = '{name}', "Module" = '{module}', "Action" = '{action}', "IsScopable" = {scopable}, "IsSystem" = TRUE, "UpdatedAt" = NOW() AT TIME ZONE 'UTC'
                    WHERE "Code" = '{code}' AND "IsDeleted" = FALSE;
                    """);
            }

            sb.AppendLine($"""
                -- System roles
                INSERT INTO "RoleEntities" ("Id","CompanyId","BranchId","Code","Name","Description","IsSystem","IsActive","CreatedBy","CreatedAt","IsDeleted","Version")
                SELECT '{RoleAdminId}'::uuid, NULL, NULL, 'ADMIN', 'Quản trị viên', 'Toàn quyền hệ thống', TRUE, TRUE, '{ActorId}'::uuid, NOW() AT TIME ZONE 'UTC', FALSE, NULL
                WHERE NOT EXISTS (SELECT 1 FROM "RoleEntities" WHERE "Code" = 'ADMIN' AND "IsDeleted" = FALSE);
                UPDATE "RoleEntities" SET "Name" = 'Quản trị viên', "Description" = 'Toàn quyền hệ thống', "IsSystem" = TRUE, "IsActive" = TRUE, "UpdatedAt" = NOW() AT TIME ZONE 'UTC'
                WHERE "Code" = 'ADMIN' AND "IsDeleted" = FALSE;

                INSERT INTO "RoleEntities" ("Id","CompanyId","BranchId","Code","Name","Description","IsSystem","IsActive","CreatedBy","CreatedAt","IsDeleted","Version")
                SELECT '{RoleHrId}'::uuid, NULL, NULL, 'HR', 'Nhân sự', 'Quản lý tổ chức, nhân viên, chấm công, nghỉ phép, hợp đồng, lương', TRUE, TRUE, '{ActorId}'::uuid, NOW() AT TIME ZONE 'UTC', FALSE, NULL
                WHERE NOT EXISTS (SELECT 1 FROM "RoleEntities" WHERE "Code" = 'HR' AND "IsDeleted" = FALSE);
                UPDATE "RoleEntities" SET "Name" = 'Nhân sự', "IsSystem" = TRUE, "IsActive" = TRUE, "UpdatedAt" = NOW() AT TIME ZONE 'UTC'
                WHERE "Code" = 'HR' AND "IsDeleted" = FALSE;

                INSERT INTO "RoleEntities" ("Id","CompanyId","BranchId","Code","Name","Description","IsSystem","IsActive","CreatedBy","CreatedAt","IsDeleted","Version")
                SELECT '{RoleManagerId}'::uuid, NULL, NULL, 'MANAGER', 'Quản lý', 'Duyệt nghỉ phép và xem dữ liệu theo chi nhánh', TRUE, TRUE, '{ActorId}'::uuid, NOW() AT TIME ZONE 'UTC', FALSE, NULL
                WHERE NOT EXISTS (SELECT 1 FROM "RoleEntities" WHERE "Code" = 'MANAGER' AND "IsDeleted" = FALSE);
                UPDATE "RoleEntities" SET "Name" = 'Quản lý', "IsSystem" = TRUE, "IsActive" = TRUE, "UpdatedAt" = NOW() AT TIME ZONE 'UTC'
                WHERE "Code" = 'MANAGER' AND "IsDeleted" = FALSE;

                INSERT INTO "RoleEntities" ("Id","CompanyId","BranchId","Code","Name","Description","IsSystem","IsActive","CreatedBy","CreatedAt","IsDeleted","Version")
                SELECT '{RoleEmployeeId}'::uuid, NULL, NULL, 'EMPLOYEE', 'Nhân viên', 'Truy cập mobile và dữ liệu cá nhân', TRUE, TRUE, '{ActorId}'::uuid, NOW() AT TIME ZONE 'UTC', FALSE, NULL
                WHERE NOT EXISTS (SELECT 1 FROM "RoleEntities" WHERE "Code" = 'EMPLOYEE' AND "IsDeleted" = FALSE);
                UPDATE "RoleEntities" SET "Name" = 'Nhân viên', "IsSystem" = TRUE, "IsActive" = TRUE, "UpdatedAt" = NOW() AT TIME ZONE 'UTC'
                WHERE "Code" = 'EMPLOYEE' AND "IsDeleted" = FALSE;
                """);

            AppendRolePermissions(sb, "ADMIN", "ALL", 1, permissions.Select(r => r.Split('|')[0]).ToArray());
            AppendRolePermissions(sb, "HR", "ALL", 2,
            [
                "ORG_VIEW","ORG_MANAGE","EMPLOYEE_VIEW","EMPLOYEE_CREATE","EMPLOYEE_UPDATE","EMPLOYEE_DELETE","EMPLOYEE_MANAGE",
                "TIMEKEEPING_VIEW","TIMEKEEPING_ADJUST","TIMEKEEPING_MANAGE","SHIFT_VIEW","SHIFT_MANAGE",
                "LEAVE_VIEW","LEAVE_CREATE","LEAVE_APPROVE","LEAVE_MANAGE","ATTENDANCE_COMPLAINT_VIEW","ATTENDANCE_COMPLAINT_REVIEW",
                "CONTRACT_VIEW","CONTRACT_CREATE","CONTRACT_UPDATE","CONTRACT_MANAGE","PAYROLL_VIEW","PAYROLL_CREATE","PAYROLL_MANAGE",
                "USER_VIEW","ACTION_LOG_VIEW","MOBILE_ACCESS"
            ]);
            AppendRolePermissions(sb, "MANAGER", "BRANCH", 3,
            [
                "LEAVE_APPROVE","TIMEKEEPING_VIEW","ATTENDANCE_COMPLAINT_VIEW","EMPLOYEE_VIEW","MOBILE_ACCESS"
            ]);
            AppendRolePermissions(sb, "EMPLOYEE", "OWN", 4,
            [
                "MOBILE_ACCESS","TIMEKEEPING_VIEW","LEAVE_CREATE","LEAVE_VIEW","PAYROLL_VIEW","ATTENDANCE_COMPLAINT_CREATE"
            ]);

            sb.AppendLine($"""
                INSERT INTO "UserRoleEntities" ("Id","UserId","RoleId","EffectiveFrom","EffectiveTo","CreatedBy","CreatedAt","IsDeleted","Version")
                SELECT '40000000-0000-0000-0000-000000000001'::uuid,
                       '{AdminUserId}'::uuid,
                       (SELECT "Id" FROM "RoleEntities" WHERE "Code" = 'ADMIN' AND "IsDeleted" = FALSE LIMIT 1),
                       NOW() AT TIME ZONE 'UTC', NULL, '{ActorId}'::uuid, NOW() AT TIME ZONE 'UTC', FALSE, NULL
                WHERE EXISTS (SELECT 1 FROM "UserEntities" WHERE "Id" = '{AdminUserId}'::uuid AND "IsDeleted" = FALSE)
                  AND NOT EXISTS (
                    SELECT 1 FROM "UserRoleEntities" ur
                    INNER JOIN "RoleEntities" r ON r."Id" = ur."RoleId"
                    WHERE ur."UserId" = '{AdminUserId}'::uuid AND r."Code" = 'ADMIN' AND ur."IsDeleted" = FALSE
                  );
                """);

            return sb.ToString();
        }

        private static void AppendRolePermissions(
            System.Text.StringBuilder sb,
            string roleCode,
            string dataScope,
            int roleNum,
            string[] codes)
        {
            sb.AppendLine($"-- RolePermission {roleCode} scope={dataScope}");
            for (int i = 0; i < codes.Length; i++)
            {
                string code = codes[i];
                string rpId = $"30000000-0000-0000-{roleNum:D4}-{(i + 1):D12}";
                sb.AppendLine($"""
                    INSERT INTO "RolePermissionEntities" ("Id","RoleId","PermissionId","DataScope","CreatedBy","CreatedAt","IsDeleted","Version")
                    SELECT '{rpId}'::uuid,
                           (SELECT "Id" FROM "RoleEntities" WHERE "Code" = '{roleCode}' AND "IsDeleted" = FALSE LIMIT 1),
                           (SELECT "Id" FROM "PermissionEntities" WHERE "Code" = '{code}' AND "IsDeleted" = FALSE LIMIT 1),
                           '{dataScope}', '{ActorId}'::uuid, NOW() AT TIME ZONE 'UTC', FALSE, NULL
                    WHERE NOT EXISTS (
                      SELECT 1 FROM "RolePermissionEntities" rp
                      INNER JOIN "RoleEntities" r ON r."Id" = rp."RoleId"
                      INNER JOIN "PermissionEntities" p ON p."Id" = rp."PermissionId"
                      WHERE r."Code" = '{roleCode}' AND p."Code" = '{code}' AND rp."IsDeleted" = FALSE
                    );
                    UPDATE "RolePermissionEntities" rp
                    SET "DataScope" = '{dataScope}', "IsDeleted" = FALSE, "UpdatedAt" = NOW() AT TIME ZONE 'UTC'
                    FROM "RoleEntities" r, "PermissionEntities" p
                    WHERE rp."RoleId" = r."Id" AND rp."PermissionId" = p."Id"
                      AND r."Code" = '{roleCode}' AND p."Code" = '{code}';
                    """);
            }
        }
    }
}
