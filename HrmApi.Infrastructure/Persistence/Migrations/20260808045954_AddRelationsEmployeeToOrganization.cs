using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationsEmployeeToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "EmployeeEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "EmployeeEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "EmployeeEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartId",
                table: "EmployeeEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "EmployeeEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEntities_BranchId",
                table: "EmployeeEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEntities_CompanyId",
                table: "EmployeeEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEntities_DepartmentId",
                table: "EmployeeEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEntities_PartId",
                table: "EmployeeEntities",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEntities_PositionId",
                table: "EmployeeEntities",
                column: "PositionId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropIndex(
                name: "IX_EmployeeEntities_BranchId",
                table: "EmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeEntities_CompanyId",
                table: "EmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeEntities_DepartmentId",
                table: "EmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeEntities_PartId",
                table: "EmployeeEntities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeEntities_PositionId",
                table: "EmployeeEntities");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "EmployeeEntities");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "EmployeeEntities");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "EmployeeEntities");

            migrationBuilder.DropColumn(
                name: "PartId",
                table: "EmployeeEntities");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "EmployeeEntities");
        }
    }
}
