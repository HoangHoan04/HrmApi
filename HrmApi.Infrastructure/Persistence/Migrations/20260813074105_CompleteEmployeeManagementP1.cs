using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteEmployeeManagementP1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "EmployeeFileEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacesFileId",
                table: "EmployeeFileEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionNo",
                table: "EmployeeFileEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DirectManagerId",
                table: "EmployeeEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeFileEntities_ReplacesFileId",
                table: "EmployeeFileEntities",
                column: "ReplacesFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEntities_DirectManagerId",
                table: "EmployeeEntities",
                column: "DirectManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEntities_EmployeeEntities_DirectManagerId",
                table: "EmployeeEntities",
                column: "DirectManagerId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeFileEntities_EmployeeFileEntities_ReplacesFileId",
                table: "EmployeeFileEntities",
                column: "ReplacesFileId",
                principalTable: "EmployeeFileEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEntities_EmployeeEntities_DirectManagerId",
                table: "EmployeeEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeFileEntities_EmployeeFileEntities_ReplacesFileId",
                table: "EmployeeFileEntities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeFileEntities_ReplacesFileId",
                table: "EmployeeFileEntities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeEntities_DirectManagerId",
                table: "EmployeeEntities");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "EmployeeFileEntities");

            migrationBuilder.DropColumn(
                name: "ReplacesFileId",
                table: "EmployeeFileEntities");

            migrationBuilder.DropColumn(
                name: "VersionNo",
                table: "EmployeeFileEntities");

            migrationBuilder.DropColumn(
                name: "DirectManagerId",
                table: "EmployeeEntities");
        }
    }
}
