using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchEntities_BranchEntities_BranchEntityId",
                table: "BranchEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchEntities_CompanyEntities_CompanyEntityId",
                table: "BranchEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyEntities_CompanyEntities_CompanyEntityId",
                table: "CompanyEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentEntities_BranchEntities_BranchEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentEntities_DepartmentEntities_DepartmentEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDependentEntities_EmployeeEntities_EmployeeEntityId",
                table: "EmployeeDependentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_BranchEntities_BranchId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_CompanyEntities_CompanyId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_DepartmentEntities_DepartmentId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_PartEntities_PartId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_PositionEntities_PositionId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartEntities_PartMasterEntities_PartMasterEntityId",
                table: "PartEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartMasterEntities_BranchEntities_BranchEntityId",
                table: "PartMasterEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionEntities_PositionMasterEntities_PositionMasterEntit~",
                table: "PositionEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionMasterEntities_PartMasterEntities_PartMasterEntityId",
                table: "PositionMasterEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleEntities_RoleEntities_RoleId",
                table: "UserRoleEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionMasterEntities_PartMasterEntityId",
                table: "PositionMasterEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionEntities_PositionMasterEntityId",
                table: "PositionEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartMasterEntities_BranchEntityId",
                table: "PartMasterEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartEntities_PartMasterEntityId",
                table: "PartEntities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDependentEntities_EmployeeEntityId",
                table: "EmployeeDependentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentEntities_BranchEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentEntities_DepartmentEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_CompanyEntities_CompanyEntityId",
                table: "CompanyEntities");

            migrationBuilder.DropIndex(
                name: "IX_BranchEntities_BranchEntityId",
                table: "BranchEntities");

            migrationBuilder.DropIndex(
                name: "IX_BranchEntities_CompanyEntityId",
                table: "BranchEntities");

            // Copy data from shadow FKs into the real FK columns before dropping shadows.
            migrationBuilder.Sql("""
                UPDATE "BranchEntities" SET "CompanyId" = "CompanyEntityId"
                WHERE "CompanyId" IS NULL AND "CompanyEntityId" IS NOT NULL;
                UPDATE "BranchEntities" SET "ParentBranchId" = "BranchEntityId"
                WHERE "ParentBranchId" IS NULL AND "BranchEntityId" IS NOT NULL;
                UPDATE "CompanyEntities" SET "ParentId" = "CompanyEntityId"
                WHERE "ParentId" IS NULL AND "CompanyEntityId" IS NOT NULL;
                UPDATE "DepartmentEntities" SET "BranchId" = "BranchEntityId"
                WHERE "BranchId" IS NULL AND "BranchEntityId" IS NOT NULL;
                UPDATE "DepartmentEntities" SET "ParentDepartmentId" = "DepartmentEntityId"
                WHERE "ParentDepartmentId" IS NULL AND "DepartmentEntityId" IS NOT NULL;
                UPDATE "EmployeeDependentEntities" SET "EmployeeId" = "EmployeeEntityId"
                WHERE "EmployeeId" = '00000000-0000-0000-0000-000000000000'
                  AND "EmployeeEntityId" IS NOT NULL;
                UPDATE "PartEntities" SET "PartMasterId" = "PartMasterEntityId"
                WHERE "PartMasterId" IS NULL AND "PartMasterEntityId" IS NOT NULL;
                UPDATE "PartMasterEntities" SET "BranchId" = "BranchEntityId"
                WHERE "BranchId" IS NULL AND "BranchEntityId" IS NOT NULL;
                UPDATE "PositionEntities" SET "PositionMasterId" = "PositionMasterEntityId"
                WHERE "PositionMasterId" IS NULL AND "PositionMasterEntityId" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "PartMasterEntityId",
                table: "PositionMasterEntities");

            migrationBuilder.DropColumn(
                name: "PositionMasterEntityId",
                table: "PositionEntities");

            migrationBuilder.DropColumn(
                name: "BranchEntityId",
                table: "PartMasterEntities");

            migrationBuilder.DropColumn(
                name: "PartMasterEntityId",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "EmployeeEntityId",
                table: "EmployeeDependentEntities");

            migrationBuilder.DropColumn(
                name: "BranchEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "DepartmentEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "CompanyEntityId",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "BranchEntityId",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "CompanyEntityId",
                table: "BranchEntities");

            migrationBuilder.RenameColumn(
                name: "Bankname",
                table: "EmployeeEntities",
                newName: "BankName");

            migrationBuilder.CreateIndex(
                name: "IX_WorkScheduledEmployeeEntities_BranchId",
                table: "WorkScheduledEmployeeEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkScheduledEmployeeEntities_ShiftId",
                table: "WorkScheduledEmployeeEntities",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkScheduledEmployeeEntities_ShiftMasterId",
                table: "WorkScheduledEmployeeEntities",
                column: "ShiftMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokenEntities_ReplacedByTokenId",
                table: "UserTokenEntities",
                column: "ReplacedByTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_BranchId",
                table: "UserEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_CompanyId",
                table: "UserEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_EmployeeId",
                table: "UserEntities",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimekeepingSummaryEntities_BranchId",
                table: "TimekeepingSummaryEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TimekeepingSummaryEntities_CompanyId",
                table: "TimekeepingSummaryEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeKeepingStandardEntities_CompanyId",
                table: "TimeKeepingStandardEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TimekeepingEntities_BranchId",
                table: "TimekeepingEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TimekeepingEntities_CompanyId",
                table: "TimekeepingEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TimekeepingEntities_ShiftId",
                table: "TimekeepingEntities",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_TimekeepingEntities_ShiftMasterId",
                table: "TimekeepingEntities",
                column: "ShiftMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftMasterEntities_CompanyId",
                table: "ShiftMasterEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftEntities_BranchId",
                table: "ShiftEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftEntities_ShiftMasterId",
                table: "ShiftEntities",
                column: "ShiftMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleEntities_BranchId",
                table: "RoleEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleEntities_CompanyId",
                table: "RoleEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterDayOffEntities_ApproverId",
                table: "RegisterDayOffEntities",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterDayOffEntities_BranchId",
                table: "RegisterDayOffEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterDayOffEntities_CompanyId",
                table: "RegisterDayOffEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterDayOffEntities_DayOffConfigId",
                table: "RegisterDayOffEntities",
                column: "DayOffConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterDayOffEntities_EmployeeId",
                table: "RegisterDayOffEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicHolidayEntities_CompanyId",
                table: "PublicHolidayEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionMasterEntities_BranchId",
                table: "PositionMasterEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionMasterEntities_CompanyId",
                table: "PositionMasterEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntities_BranchId",
                table: "PositionEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntities_CompanyId",
                table: "PositionEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntities_DepartmentId",
                table: "PositionEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntities_PartId",
                table: "PositionEntities",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntities_PositionMasterId",
                table: "PositionEntities",
                column: "PositionMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_PartMasterEntities_BranchId",
                table: "PartMasterEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PartMasterEntities_CompanyId",
                table: "PartMasterEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartEntities_BranchId",
                table: "PartEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PartEntities_CompanyId",
                table: "PartEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartEntities_DepartmentId",
                table: "PartEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PartEntities_ManagerId",
                table: "PartEntities",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_PartEntities_PartMasterId",
                table: "PartEntities",
                column: "PartMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDependentEntities_EmployeeId",
                table: "EmployeeDependentEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_BranchId",
                table: "DepartmentEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_CompanyId",
                table: "DepartmentEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_DeputyManagerId",
                table: "DepartmentEntities",
                column: "DeputyManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_ManagerId",
                table: "DepartmentEntities",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_ParentDepartmentId",
                table: "DepartmentEntities",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffConfigEntities_CompanyId",
                table: "DayOffConfigEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffConfigEmployeeEntities_DayOffConfigId",
                table: "DayOffConfigEmployeeEntities",
                column: "DayOffConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffConfigEmployeeEntities_EmployeeId",
                table: "DayOffConfigEmployeeEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEntities_ParentId",
                table: "CompanyEntities",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEntities_TimeKeepingStandardId",
                table: "CompanyEntities",
                column: "TimeKeepingStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchEntities_CompanyId",
                table: "BranchEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchEntities_ManagerId",
                table: "BranchEntities",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchEntities_ParentBranchId",
                table: "BranchEntities",
                column: "ParentBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchEntities_TimeKeepingStandardId",
                table: "BranchEntities",
                column: "TimeKeepingStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogEntities_CreatedById",
                table: "ActionLogEntities",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogEntities_EntityId",
                table: "ActionLogEntities",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchEntities_BranchEntities_ParentBranchId",
                table: "BranchEntities",
                column: "ParentBranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchEntities_CompanyEntities_CompanyId",
                table: "BranchEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchEntities_EmployeeEntities_ManagerId",
                table: "BranchEntities",
                column: "ManagerId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchEntities_TimeKeepingStandardEntities_TimeKeepingStand~",
                table: "BranchEntities",
                column: "TimeKeepingStandardId",
                principalTable: "TimeKeepingStandardEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyEntities_CompanyEntities_ParentId",
                table: "CompanyEntities",
                column: "ParentId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyEntities_TimeKeepingStandardEntities_TimeKeepingStan~",
                table: "CompanyEntities",
                column: "TimeKeepingStandardId",
                principalTable: "TimeKeepingStandardEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffConfigEmployeeEntities_DayOffConfigEntities_DayOffCon~",
                table: "DayOffConfigEmployeeEntities",
                column: "DayOffConfigId",
                principalTable: "DayOffConfigEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffConfigEmployeeEntities_EmployeeEntities_EmployeeId",
                table: "DayOffConfigEmployeeEntities",
                column: "EmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffConfigEntities_CompanyEntities_CompanyId",
                table: "DayOffConfigEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentEntities_BranchEntities_BranchId",
                table: "DepartmentEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentEntities_CompanyEntities_CompanyId",
                table: "DepartmentEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentEntities_DepartmentEntities_ParentDepartmentId",
                table: "DepartmentEntities",
                column: "ParentDepartmentId",
                principalTable: "DepartmentEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentEntities_EmployeeEntities_DeputyManagerId",
                table: "DepartmentEntities",
                column: "DeputyManagerId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentEntities_EmployeeEntities_ManagerId",
                table: "DepartmentEntities",
                column: "ManagerId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDependentEntities_EmployeeEntities_EmployeeId",
                table: "EmployeeDependentEntities",
                column: "EmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_BranchEntities_BranchId",
                table: "EmployeeEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_CompanyEntities_CompanyId",
                table: "EmployeeEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_DepartmentEntities_DepartmentId",
                table: "EmployeeEntities",
                column: "DepartmentId",
                principalTable: "DepartmentEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_PartEntities_PartId",
                table: "EmployeeEntities",
                column: "PartId",
                principalTable: "PartEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_PositionEntities_PositionId",
                table: "EmployeeEntities",
                column: "PositionId",
                principalTable: "PositionEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartEntities_BranchEntities_BranchId",
                table: "PartEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartEntities_CompanyEntities_CompanyId",
                table: "PartEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartEntities_DepartmentEntities_DepartmentId",
                table: "PartEntities",
                column: "DepartmentId",
                principalTable: "DepartmentEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartEntities_EmployeeEntities_ManagerId",
                table: "PartEntities",
                column: "ManagerId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartEntities_PartMasterEntities_PartMasterId",
                table: "PartEntities",
                column: "PartMasterId",
                principalTable: "PartMasterEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartMasterEntities_BranchEntities_BranchId",
                table: "PartMasterEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartMasterEntities_CompanyEntities_CompanyId",
                table: "PartMasterEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionEntities_BranchEntities_BranchId",
                table: "PositionEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionEntities_CompanyEntities_CompanyId",
                table: "PositionEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionEntities_DepartmentEntities_DepartmentId",
                table: "PositionEntities",
                column: "DepartmentId",
                principalTable: "DepartmentEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionEntities_PartEntities_PartId",
                table: "PositionEntities",
                column: "PartId",
                principalTable: "PartEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionEntities_PositionMasterEntities_PositionMasterId",
                table: "PositionEntities",
                column: "PositionMasterId",
                principalTable: "PositionMasterEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionMasterEntities_BranchEntities_BranchId",
                table: "PositionMasterEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionMasterEntities_CompanyEntities_CompanyId",
                table: "PositionMasterEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicHolidayEntities_CompanyEntities_CompanyId",
                table: "PublicHolidayEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterDayOffEntities_BranchEntities_BranchId",
                table: "RegisterDayOffEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterDayOffEntities_CompanyEntities_CompanyId",
                table: "RegisterDayOffEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterDayOffEntities_DayOffConfigEntities_DayOffConfigId",
                table: "RegisterDayOffEntities",
                column: "DayOffConfigId",
                principalTable: "DayOffConfigEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterDayOffEntities_EmployeeEntities_ApproverId",
                table: "RegisterDayOffEntities",
                column: "ApproverId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterDayOffEntities_EmployeeEntities_EmployeeId",
                table: "RegisterDayOffEntities",
                column: "EmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleEntities_BranchEntities_BranchId",
                table: "RoleEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleEntities_CompanyEntities_CompanyId",
                table: "RoleEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities",
                column: "PermissionId",
                principalTable: "PermissionEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftEntities_BranchEntities_BranchId",
                table: "ShiftEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftEntities_ShiftMasterEntities_ShiftMasterId",
                table: "ShiftEntities",
                column: "ShiftMasterId",
                principalTable: "ShiftMasterEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftMasterEntities_CompanyEntities_CompanyId",
                table: "ShiftMasterEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimekeepingEntities_BranchEntities_BranchId",
                table: "TimekeepingEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimekeepingEntities_CompanyEntities_CompanyId",
                table: "TimekeepingEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimekeepingEntities_EmployeeEntities_EmployeeId",
                table: "TimekeepingEntities",
                column: "EmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimekeepingEntities_ShiftEntities_ShiftId",
                table: "TimekeepingEntities",
                column: "ShiftId",
                principalTable: "ShiftEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimekeepingEntities_ShiftMasterEntities_ShiftMasterId",
                table: "TimekeepingEntities",
                column: "ShiftMasterId",
                principalTable: "ShiftMasterEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeKeepingStandardEntities_CompanyEntities_CompanyId",
                table: "TimeKeepingStandardEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimekeepingSummaryEntities_BranchEntities_BranchId",
                table: "TimekeepingSummaryEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimekeepingSummaryEntities_CompanyEntities_CompanyId",
                table: "TimekeepingSummaryEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimekeepingSummaryEntities_EmployeeEntities_EmployeeId",
                table: "TimekeepingSummaryEntities",
                column: "EmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEntities_BranchEntities_BranchId",
                table: "UserEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEntities_CompanyEntities_CompanyId",
                table: "UserEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEntities_EmployeeEntities_EmployeeId",
                table: "UserEntities",
                column: "EmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleEntities_RoleEntities_RoleId",
                table: "UserRoleEntities",
                column: "RoleId",
                principalTable: "RoleEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokenEntities_UserTokenEntities_ReplacedByTokenId",
                table: "UserTokenEntities",
                column: "ReplacedByTokenId",
                principalTable: "UserTokenEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkScheduledEmployeeEntities_BranchEntities_BranchId",
                table: "WorkScheduledEmployeeEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkScheduledEmployeeEntities_EmployeeEntities_EmployeeId",
                table: "WorkScheduledEmployeeEntities",
                column: "EmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkScheduledEmployeeEntities_ShiftEntities_ShiftId",
                table: "WorkScheduledEmployeeEntities",
                column: "ShiftId",
                principalTable: "ShiftEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkScheduledEmployeeEntities_ShiftMasterEntities_ShiftMast~",
                table: "WorkScheduledEmployeeEntities",
                column: "ShiftMasterId",
                principalTable: "ShiftMasterEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchEntities_BranchEntities_ParentBranchId",
                table: "BranchEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchEntities_CompanyEntities_CompanyId",
                table: "BranchEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchEntities_EmployeeEntities_ManagerId",
                table: "BranchEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchEntities_TimeKeepingStandardEntities_TimeKeepingStand~",
                table: "BranchEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyEntities_CompanyEntities_ParentId",
                table: "CompanyEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyEntities_TimeKeepingStandardEntities_TimeKeepingStan~",
                table: "CompanyEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DayOffConfigEmployeeEntities_DayOffConfigEntities_DayOffCon~",
                table: "DayOffConfigEmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DayOffConfigEmployeeEntities_EmployeeEntities_EmployeeId",
                table: "DayOffConfigEmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DayOffConfigEntities_CompanyEntities_CompanyId",
                table: "DayOffConfigEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentEntities_BranchEntities_BranchId",
                table: "DepartmentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentEntities_CompanyEntities_CompanyId",
                table: "DepartmentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentEntities_DepartmentEntities_ParentDepartmentId",
                table: "DepartmentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentEntities_EmployeeEntities_DeputyManagerId",
                table: "DepartmentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentEntities_EmployeeEntities_ManagerId",
                table: "DepartmentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDependentEntities_EmployeeEntities_EmployeeId",
                table: "EmployeeDependentEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_BranchEntities_BranchId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_CompanyEntities_CompanyId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_DepartmentEntities_DepartmentId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_PartEntities_PartId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_PositionEntities_PositionId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartEntities_BranchEntities_BranchId",
                table: "PartEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartEntities_CompanyEntities_CompanyId",
                table: "PartEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartEntities_DepartmentEntities_DepartmentId",
                table: "PartEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartEntities_EmployeeEntities_ManagerId",
                table: "PartEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartEntities_PartMasterEntities_PartMasterId",
                table: "PartEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartMasterEntities_BranchEntities_BranchId",
                table: "PartMasterEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PartMasterEntities_CompanyEntities_CompanyId",
                table: "PartMasterEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionEntities_BranchEntities_BranchId",
                table: "PositionEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionEntities_CompanyEntities_CompanyId",
                table: "PositionEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionEntities_DepartmentEntities_DepartmentId",
                table: "PositionEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionEntities_PartEntities_PartId",
                table: "PositionEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionEntities_PositionMasterEntities_PositionMasterId",
                table: "PositionEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionMasterEntities_BranchEntities_BranchId",
                table: "PositionMasterEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionMasterEntities_CompanyEntities_CompanyId",
                table: "PositionMasterEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicHolidayEntities_CompanyEntities_CompanyId",
                table: "PublicHolidayEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterDayOffEntities_BranchEntities_BranchId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterDayOffEntities_CompanyEntities_CompanyId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterDayOffEntities_DayOffConfigEntities_DayOffConfigId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterDayOffEntities_EmployeeEntities_ApproverId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterDayOffEntities_EmployeeEntities_EmployeeId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleEntities_BranchEntities_BranchId",
                table: "RoleEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleEntities_CompanyEntities_CompanyId",
                table: "RoleEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftEntities_BranchEntities_BranchId",
                table: "ShiftEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftEntities_ShiftMasterEntities_ShiftMasterId",
                table: "ShiftEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftMasterEntities_CompanyEntities_CompanyId",
                table: "ShiftMasterEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimekeepingEntities_BranchEntities_BranchId",
                table: "TimekeepingEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimekeepingEntities_CompanyEntities_CompanyId",
                table: "TimekeepingEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimekeepingEntities_EmployeeEntities_EmployeeId",
                table: "TimekeepingEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimekeepingEntities_ShiftEntities_ShiftId",
                table: "TimekeepingEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimekeepingEntities_ShiftMasterEntities_ShiftMasterId",
                table: "TimekeepingEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeKeepingStandardEntities_CompanyEntities_CompanyId",
                table: "TimeKeepingStandardEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimekeepingSummaryEntities_BranchEntities_BranchId",
                table: "TimekeepingSummaryEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimekeepingSummaryEntities_CompanyEntities_CompanyId",
                table: "TimekeepingSummaryEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TimekeepingSummaryEntities_EmployeeEntities_EmployeeId",
                table: "TimekeepingSummaryEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEntities_BranchEntities_BranchId",
                table: "UserEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEntities_CompanyEntities_CompanyId",
                table: "UserEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEntities_EmployeeEntities_EmployeeId",
                table: "UserEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleEntities_RoleEntities_RoleId",
                table: "UserRoleEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokenEntities_UserTokenEntities_ReplacedByTokenId",
                table: "UserTokenEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkScheduledEmployeeEntities_BranchEntities_BranchId",
                table: "WorkScheduledEmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkScheduledEmployeeEntities_EmployeeEntities_EmployeeId",
                table: "WorkScheduledEmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkScheduledEmployeeEntities_ShiftEntities_ShiftId",
                table: "WorkScheduledEmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkScheduledEmployeeEntities_ShiftMasterEntities_ShiftMast~",
                table: "WorkScheduledEmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_WorkScheduledEmployeeEntities_BranchId",
                table: "WorkScheduledEmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_WorkScheduledEmployeeEntities_ShiftId",
                table: "WorkScheduledEmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_WorkScheduledEmployeeEntities_ShiftMasterId",
                table: "WorkScheduledEmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserTokenEntities_ReplacedByTokenId",
                table: "UserTokenEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserEntities_BranchId",
                table: "UserEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserEntities_CompanyId",
                table: "UserEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserEntities_EmployeeId",
                table: "UserEntities");

            migrationBuilder.DropIndex(
                name: "IX_TimekeepingSummaryEntities_BranchId",
                table: "TimekeepingSummaryEntities");

            migrationBuilder.DropIndex(
                name: "IX_TimekeepingSummaryEntities_CompanyId",
                table: "TimekeepingSummaryEntities");

            migrationBuilder.DropIndex(
                name: "IX_TimeKeepingStandardEntities_CompanyId",
                table: "TimeKeepingStandardEntities");

            migrationBuilder.DropIndex(
                name: "IX_TimekeepingEntities_BranchId",
                table: "TimekeepingEntities");

            migrationBuilder.DropIndex(
                name: "IX_TimekeepingEntities_CompanyId",
                table: "TimekeepingEntities");

            migrationBuilder.DropIndex(
                name: "IX_TimekeepingEntities_ShiftId",
                table: "TimekeepingEntities");

            migrationBuilder.DropIndex(
                name: "IX_TimekeepingEntities_ShiftMasterId",
                table: "TimekeepingEntities");

            migrationBuilder.DropIndex(
                name: "IX_ShiftMasterEntities_CompanyId",
                table: "ShiftMasterEntities");

            migrationBuilder.DropIndex(
                name: "IX_ShiftEntities_BranchId",
                table: "ShiftEntities");

            migrationBuilder.DropIndex(
                name: "IX_ShiftEntities_ShiftMasterId",
                table: "ShiftEntities");

            migrationBuilder.DropIndex(
                name: "IX_RoleEntities_BranchId",
                table: "RoleEntities");

            migrationBuilder.DropIndex(
                name: "IX_RoleEntities_CompanyId",
                table: "RoleEntities");

            migrationBuilder.DropIndex(
                name: "IX_RegisterDayOffEntities_ApproverId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropIndex(
                name: "IX_RegisterDayOffEntities_BranchId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropIndex(
                name: "IX_RegisterDayOffEntities_CompanyId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropIndex(
                name: "IX_RegisterDayOffEntities_DayOffConfigId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropIndex(
                name: "IX_RegisterDayOffEntities_EmployeeId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropIndex(
                name: "IX_PublicHolidayEntities_CompanyId",
                table: "PublicHolidayEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionMasterEntities_BranchId",
                table: "PositionMasterEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionMasterEntities_CompanyId",
                table: "PositionMasterEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionEntities_BranchId",
                table: "PositionEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionEntities_CompanyId",
                table: "PositionEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionEntities_DepartmentId",
                table: "PositionEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionEntities_PartId",
                table: "PositionEntities");

            migrationBuilder.DropIndex(
                name: "IX_PositionEntities_PositionMasterId",
                table: "PositionEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartMasterEntities_BranchId",
                table: "PartMasterEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartMasterEntities_CompanyId",
                table: "PartMasterEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartEntities_BranchId",
                table: "PartEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartEntities_CompanyId",
                table: "PartEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartEntities_DepartmentId",
                table: "PartEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartEntities_ManagerId",
                table: "PartEntities");

            migrationBuilder.DropIndex(
                name: "IX_PartEntities_PartMasterId",
                table: "PartEntities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDependentEntities_EmployeeId",
                table: "EmployeeDependentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentEntities_BranchId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentEntities_CompanyId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentEntities_DeputyManagerId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentEntities_ManagerId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentEntities_ParentDepartmentId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DayOffConfigEntities_CompanyId",
                table: "DayOffConfigEntities");

            migrationBuilder.DropIndex(
                name: "IX_DayOffConfigEmployeeEntities_DayOffConfigId",
                table: "DayOffConfigEmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_DayOffConfigEmployeeEntities_EmployeeId",
                table: "DayOffConfigEmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_CompanyEntities_ParentId",
                table: "CompanyEntities");

            migrationBuilder.DropIndex(
                name: "IX_CompanyEntities_TimeKeepingStandardId",
                table: "CompanyEntities");

            migrationBuilder.DropIndex(
                name: "IX_BranchEntities_CompanyId",
                table: "BranchEntities");

            migrationBuilder.DropIndex(
                name: "IX_BranchEntities_ManagerId",
                table: "BranchEntities");

            migrationBuilder.DropIndex(
                name: "IX_BranchEntities_ParentBranchId",
                table: "BranchEntities");

            migrationBuilder.DropIndex(
                name: "IX_BranchEntities_TimeKeepingStandardId",
                table: "BranchEntities");

            migrationBuilder.DropIndex(
                name: "IX_ActionLogEntities_CreatedById",
                table: "ActionLogEntities");

            migrationBuilder.DropIndex(
                name: "IX_ActionLogEntities_EntityId",
                table: "ActionLogEntities");

            migrationBuilder.RenameColumn(
                name: "BankName",
                table: "EmployeeEntities",
                newName: "Bankname");

            migrationBuilder.AddColumn<Guid>(
                name: "PartMasterEntityId",
                table: "PositionMasterEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionMasterEntityId",
                table: "PositionEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchEntityId",
                table: "PartMasterEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartMasterEntityId",
                table: "PartEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeEntityId",
                table: "EmployeeDependentEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchEntityId",
                table: "DepartmentEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentEntityId",
                table: "DepartmentEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyEntityId",
                table: "CompanyEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchEntityId",
                table: "BranchEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyEntityId",
                table: "BranchEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionMasterEntities_PartMasterEntityId",
                table: "PositionMasterEntities",
                column: "PartMasterEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntities_PositionMasterEntityId",
                table: "PositionEntities",
                column: "PositionMasterEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PartMasterEntities_BranchEntityId",
                table: "PartMasterEntities",
                column: "BranchEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PartEntities_PartMasterEntityId",
                table: "PartEntities",
                column: "PartMasterEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDependentEntities_EmployeeEntityId",
                table: "EmployeeDependentEntities",
                column: "EmployeeEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_BranchEntityId",
                table: "DepartmentEntities",
                column: "BranchEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_DepartmentEntityId",
                table: "DepartmentEntities",
                column: "DepartmentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEntities_CompanyEntityId",
                table: "CompanyEntities",
                column: "CompanyEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchEntities_BranchEntityId",
                table: "BranchEntities",
                column: "BranchEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchEntities_CompanyEntityId",
                table: "BranchEntities",
                column: "CompanyEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchEntities_BranchEntities_BranchEntityId",
                table: "BranchEntities",
                column: "BranchEntityId",
                principalTable: "BranchEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchEntities_CompanyEntities_CompanyEntityId",
                table: "BranchEntities",
                column: "CompanyEntityId",
                principalTable: "CompanyEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyEntities_CompanyEntities_CompanyEntityId",
                table: "CompanyEntities",
                column: "CompanyEntityId",
                principalTable: "CompanyEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentEntities_BranchEntities_BranchEntityId",
                table: "DepartmentEntities",
                column: "BranchEntityId",
                principalTable: "BranchEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentEntities_DepartmentEntities_DepartmentEntityId",
                table: "DepartmentEntities",
                column: "DepartmentEntityId",
                principalTable: "DepartmentEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDependentEntities_EmployeeEntities_EmployeeEntityId",
                table: "EmployeeDependentEntities",
                column: "EmployeeEntityId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_BranchEntities_BranchId",
                table: "EmployeeEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_CompanyEntities_CompanyId",
                table: "EmployeeEntities",
                column: "CompanyId",
                principalTable: "CompanyEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_DepartmentEntities_DepartmentId",
                table: "EmployeeEntities",
                column: "DepartmentId",
                principalTable: "DepartmentEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_PartEntities_PartId",
                table: "EmployeeEntities",
                column: "PartId",
                principalTable: "PartEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_PositionEntities_PositionId",
                table: "EmployeeEntities",
                column: "PositionId",
                principalTable: "PositionEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartEntities_PartMasterEntities_PartMasterEntityId",
                table: "PartEntities",
                column: "PartMasterEntityId",
                principalTable: "PartMasterEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartMasterEntities_BranchEntities_BranchEntityId",
                table: "PartMasterEntities",
                column: "BranchEntityId",
                principalTable: "BranchEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PositionEntities_PositionMasterEntities_PositionMasterEntit~",
                table: "PositionEntities",
                column: "PositionMasterEntityId",
                principalTable: "PositionMasterEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PositionMasterEntities_PartMasterEntities_PartMasterEntityId",
                table: "PositionMasterEntities",
                column: "PartMasterEntityId",
                principalTable: "PartMasterEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities",
                column: "PermissionId",
                principalTable: "PermissionEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleEntities_RoleEntities_RoleId",
                table: "UserRoleEntities",
                column: "RoleId",
                principalTable: "RoleEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
