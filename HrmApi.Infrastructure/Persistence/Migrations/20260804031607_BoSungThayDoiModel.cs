using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BoSungThayDoiModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "UserTokenEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "UserRoleEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "UserEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "RolePermissionEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "RoleEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "PositionMasterEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "PositionEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "PermissionEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "PartMasterEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "PartEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "DepartmentEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "ActionLogEntities",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "UserTokenEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "UserRoleEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "UserEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "RolePermissionEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "RoleEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PositionMasterEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PositionEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PermissionEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PartMasterEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ActionLogEntities");
        }
    }
}
