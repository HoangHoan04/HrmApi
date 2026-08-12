using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropPermissionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities");

            migrationBuilder.DropTable(
                name: "PermissionEntities");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissionEntities_PermissionId",
                table: "RolePermissionEntities");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "RolePermissionEntities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId",
                table: "RolePermissionEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PermissionEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsScopable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    Module = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionEntities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionEntities_PermissionId",
                table: "RolePermissionEntities",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissionEntities_PermissionEntities_PermissionId",
                table: "RolePermissionEntities",
                column: "PermissionId",
                principalTable: "PermissionEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
