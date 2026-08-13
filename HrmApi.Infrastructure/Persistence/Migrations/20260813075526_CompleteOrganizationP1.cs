using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteOrganizationP1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GradeCode",
                table: "PositionMasterEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeName",
                table: "PositionMasterEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryMax",
                table: "PositionMasterEntities",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryMin",
                table: "PositionMasterEntities",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradeCode",
                table: "PositionMasterEntities");

            migrationBuilder.DropColumn(
                name: "GradeName",
                table: "PositionMasterEntities");

            migrationBuilder.DropColumn(
                name: "SalaryMax",
                table: "PositionMasterEntities");

            migrationBuilder.DropColumn(
                name: "SalaryMin",
                table: "PositionMasterEntities");
        }
    }
}
