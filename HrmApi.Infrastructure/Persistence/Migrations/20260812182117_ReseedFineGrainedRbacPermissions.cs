using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Remap legacy RolePermission codes → fine-grained catalog.
    /// System role packs are managed manually via Admin/API.
    /// </remarks>
    public partial class ReseedFineGrainedRbacPermissions : Migration
    {
        private const string ActorId = "00000000-0000-0000-0000-000000000000";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildRemapSql());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only expansion; irreversible.
        }

        private static string BuildRemapSql()
        {
            string[] maps =
            [
                "ORG_VIEW|ORGANIZATION_VIEW",
                "ORG_MANAGE|ORGANIZATION_MANAGE",
                "EMPLOYEE_VIEW|HUMAN_RESOURCE_EMPLOYEE_VIEW",
                "EMPLOYEE_CREATE|HUMAN_RESOURCE_EMPLOYEE_CREATE",
                "EMPLOYEE_UPDATE|HUMAN_RESOURCE_EMPLOYEE_UPDATE",
                "EMPLOYEE_DELETE|HUMAN_RESOURCE_EMPLOYEE_DEACTIVATE",
                "EMPLOYEE_MANAGE|HUMAN_RESOURCE_EMPLOYEE_MANAGE",
                "TIMEKEEPING_VIEW|OPERATE_TIMEKEEPING_VIEW",
                "TIMEKEEPING_ADJUST|OPERATE_TIMEKEEPING_ADJUST",
                "TIMEKEEPING_MANAGE|OPERATE_TIMEKEEPING_MANAGE",
                "SHIFT_VIEW|OPERATE_SHIFT_VIEW",
                "SHIFT_CREATE|OPERATE_SHIFT_CREATE",
                "SHIFT_UPDATE|OPERATE_SHIFT_UPDATE",
                "SHIFT_MANAGE|OPERATE_SHIFT_MANAGE",
                "LEAVE_VIEW|OPERATE_LEAVE_VIEW",
                "LEAVE_CREATE|OPERATE_LEAVE_CREATE",
                "LEAVE_APPROVE|OPERATE_LEAVE_APPROVE",
                "LEAVE_MANAGE|OPERATE_LEAVE_MANAGE",
                "ATTENDANCE_COMPLAINT_VIEW|OPERATE_ATTENDANCE_COMPLAINT_VIEW",
                "ATTENDANCE_COMPLAINT_CREATE|OPERATE_ATTENDANCE_COMPLAINT_CREATE",
                "ATTENDANCE_COMPLAINT_REVIEW|OPERATE_ATTENDANCE_COMPLAINT_REVIEW",
                "CONTRACT_VIEW|HUMAN_RESOURCE_CONTRACT_VIEW",
                "CONTRACT_CREATE|HUMAN_RESOURCE_CONTRACT_CREATE",
                "CONTRACT_UPDATE|HUMAN_RESOURCE_CONTRACT_UPDATE",
                "CONTRACT_MANAGE|HUMAN_RESOURCE_CONTRACT_MANAGE",
                "PAYROLL_CREATE|PAYROLL_SALARY_CREATE",
            ];

            var sb = new StringBuilder();
            sb.AppendLine("-- Remap legacy PermissionCode values (custom roles)");
            foreach (string row in maps)
            {
                string[] p = row.Split('|');
                sb.AppendLine($"""
                    UPDATE "RolePermissionEntities"
                    SET "PermissionCode" = '{p[1]}', "UpdatedAt" = NOW(), "UpdatedBy" = '{ActorId}'::uuid
                    WHERE "PermissionCode" = '{p[0]}' AND "IsDeleted" = FALSE;
                    """);
            }

            sb.AppendLine("""
                DELETE FROM "RolePermissionEntities" rp
                WHERE rp."IsDeleted" = FALSE
                  AND rp."Id" NOT IN (
                    SELECT DISTINCT ON ("RoleId", "PermissionCode") "Id"
                    FROM "RolePermissionEntities"
                    WHERE "IsDeleted" = FALSE
                    ORDER BY "RoleId", "PermissionCode", "CreatedAt"
                  );
                """);

            return sb.ToString();
        }
    }
}
