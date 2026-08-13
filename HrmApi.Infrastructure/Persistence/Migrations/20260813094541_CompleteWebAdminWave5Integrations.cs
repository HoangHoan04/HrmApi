using System;
using System.Linq;
using System.Text;
using HrmApi.Application.Common.Constants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteWebAdminWave5Integrations : Migration
    {
        private const string ActorId = "00000000-0000-0000-0000-000000000000";
        private const string RoleAdminId = "10000000-0000-0000-0000-000000000001";
        private const string RoleHrId = "10000000-0000-0000-0000-000000000002";
        private const string RoleManagerId = "10000000-0000-0000-0000-000000000003";
        private const string RoleEmployeeId = "10000000-0000-0000-0000-000000000004";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Schema for User 2FA/SSO, IpAllowlist, Sms, Zalo already applied in Wave4.
            // Wave5 only reseeds RBAC packs to include INTEGRATIONS_* permissions.
            migrationBuilder.Sql(BuildReseedSystemRolesSql());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        private static string BuildReseedSystemRolesSql()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"""
                DELETE FROM "RolePermissionEntities"
                WHERE "RoleId" IN (
                    '{RoleAdminId}'::uuid,
                    '{RoleHrId}'::uuid,
                    '{RoleManagerId}'::uuid,
                    '{RoleEmployeeId}'::uuid
                );
                """);

            AppendRolePack(sb, RoleAdminId, DataScopes.All, PermissionCodes.All);
            AppendRolePack(sb, RoleHrId, DataScopes.All, RbacPermissionCatalog.HrCodes);
            AppendRolePack(sb, RoleManagerId, DataScopes.Branch, RbacPermissionCatalog.ManagerCodes);
            AppendRolePack(sb, RoleEmployeeId, DataScopes.Own, RbacPermissionCatalog.EmployeeCodes);
            return sb.ToString();
        }

        private static void AppendRolePack(StringBuilder sb, string roleId, string dataScope, string[] codes)
        {
            foreach (string code in codes.Distinct(StringComparer.Ordinal))
            {
                sb.AppendLine($"""
                    INSERT INTO "RolePermissionEntities"
                        ("Id","RoleId","PermissionCode","DataScope","CreatedBy","CreatedAt","IsDeleted","Version")
                    SELECT gen_random_uuid(), '{roleId}'::uuid, '{code}', '{dataScope}', '{ActorId}'::uuid, NOW(), FALSE, 1
                    WHERE NOT EXISTS (
                        SELECT 1 FROM "RolePermissionEntities"
                        WHERE "RoleId" = '{roleId}'::uuid
                          AND "PermissionCode" = '{code}'
                          AND "IsDeleted" = FALSE
                    );
                    """);
            }
        }
    }
}
