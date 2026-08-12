using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RolePermissionUsePermissionCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissionEntities_RoleId",
                table: "RolePermissionEntities");

            migrationBuilder.AddColumn<string>(
                name: "PermissionCode",
                table: "RolePermissionEntities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Backfill PermissionCode from PermissionEntities before enforcing uniqueness.
            migrationBuilder.Sql("""
                UPDATE "RolePermissionEntities" rp
                SET "PermissionCode" = p."Code"
                FROM "PermissionEntities" p
                WHERE rp."PermissionId" = p."Id"
                  AND (rp."PermissionCode" IS NULL OR rp."PermissionCode" = '');
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "PermissionId",
                table: "RolePermissionEntities",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionEntities_RoleId_PermissionCode",
                table: "RolePermissionEntities",
                columns: new[] { "RoleId", "PermissionCode" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities",
                column: "PermissionId",
                principalTable: "PermissionEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissionEntities_RoleId_PermissionCode",
                table: "RolePermissionEntities");

            migrationBuilder.DropColumn(
                name: "PermissionCode",
                table: "RolePermissionEntities");

            migrationBuilder.AlterColumn<Guid>(
                name: "PermissionId",
                table: "RolePermissionEntities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionEntities_RoleId",
                table: "RolePermissionEntities",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities",
                column: "PermissionId",
                principalTable: "PermissionEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
