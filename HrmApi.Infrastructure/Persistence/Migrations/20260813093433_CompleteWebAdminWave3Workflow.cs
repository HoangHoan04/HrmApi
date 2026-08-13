using System;
using System.Linq;
using System.Text;
using HrmApi.Application.Common.Constants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteWebAdminWave3Workflow : Migration
    {
        private const string ActorId = "00000000-0000-0000-0000-000000000000";
        private const string RoleAdminId = "10000000-0000-0000-0000-000000000001";
        private const string RoleHrId = "10000000-0000-0000-0000-000000000002";
        private const string RoleManagerId = "10000000-0000-0000-0000-000000000003";
        private const string RoleEmployeeId = "10000000-0000-0000-0000-000000000004";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowDefinitionEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitionEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowDefinitionEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowFormTemplateEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchemaJson = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowFormTemplateEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentStepOrder = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceEntities_WorkflowDefinitionEntities_Definit~",
                        column: x => x.DefinitionId,
                        principalTable: "WorkflowDefinitionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStepEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApproverResolver = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequiredRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsFinal = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStepEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStepEntities_WorkflowDefinitionEntities_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "WorkflowDefinitionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTaskEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    AssigneeEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ActedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTaskEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTaskEntities_WorkflowInstanceEntities_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "WorkflowInstanceEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitionEntities_Code",
                table: "WorkflowDefinitionEntities",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitionEntities_CompanyId",
                table: "WorkflowDefinitionEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitionEntities_EntityType_CompanyId_IsActive",
                table: "WorkflowDefinitionEntities",
                columns: new[] { "EntityType", "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFormTemplateEntities_EntityType",
                table: "WorkflowFormTemplateEntities",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceEntities_DefinitionId",
                table: "WorkflowInstanceEntities",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceEntities_EntityType_EntityId",
                table: "WorkflowInstanceEntities",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceEntities_Status",
                table: "WorkflowInstanceEntities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepEntities_DefinitionId_StepOrder",
                table: "WorkflowStepEntities",
                columns: new[] { "DefinitionId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTaskEntities_AssigneeEmployeeId",
                table: "WorkflowTaskEntities",
                column: "AssigneeEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTaskEntities_InstanceId_Status",
                table: "WorkflowTaskEntities",
                columns: new[] { "InstanceId", "Status" });

            migrationBuilder.Sql(BuildReseedSystemRolesSql());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowFormTemplateEntities");

            migrationBuilder.DropTable(
                name: "WorkflowStepEntities");

            migrationBuilder.DropTable(
                name: "WorkflowTaskEntities");

            migrationBuilder.DropTable(
                name: "WorkflowInstanceEntities");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitionEntities");
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
