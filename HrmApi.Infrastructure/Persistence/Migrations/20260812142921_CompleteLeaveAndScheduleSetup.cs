using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteLeaveAndScheduleSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "RegisterDayOffEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedApproverId",
                table: "RegisterDayOffEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Session",
                table: "RegisterDayOffEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "DeductBalance",
                table: "DayOffConfigEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDaysPerRequest",
                table: "DayOffConfigEntities",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinNoticeDays",
                table: "DayOffConfigEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                table: "DayOffConfigEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SaturdayPolicy",
                table: "CompanyEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RegisterDayOffEntities_RequestedApproverId",
                table: "RegisterDayOffEntities",
                column: "RequestedApproverId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterDayOffEntities_EmployeeEntities_RequestedApproverId",
                table: "RegisterDayOffEntities",
                column: "RequestedApproverId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegisterDayOffEntities_EmployeeEntities_RequestedApproverId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropIndex(
                name: "IX_RegisterDayOffEntities_RequestedApproverId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropColumn(
                name: "RequestedApproverId",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropColumn(
                name: "Session",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropColumn(
                name: "DeductBalance",
                table: "DayOffConfigEntities");

            migrationBuilder.DropColumn(
                name: "MaxDaysPerRequest",
                table: "DayOffConfigEntities");

            migrationBuilder.DropColumn(
                name: "MinNoticeDays",
                table: "DayOffConfigEntities");

            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                table: "DayOffConfigEntities");

            migrationBuilder.DropColumn(
                name: "SaturdayPolicy",
                table: "CompanyEntities");
        }
    }
}
