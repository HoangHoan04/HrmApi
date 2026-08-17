using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Legacy seed for PermissionEntities (superseded by code catalog + DropPermissionEntity).
    /// Roles and role permissions are created manually via Admin/API.
    /// </remarks>
    public partial class SeedRbacPermissionsAndRoles : Migration
    {
        private const string ActorId = "00000000-0000-0000-0000-000000000000";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildSeedSql());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
            sb.AppendLine("-- Permissions (legacy table; dropped in DropPermissionEntity)");
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

            return sb.ToString();
        }
    }
}
