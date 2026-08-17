using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropDayOffTypeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOffType",
                table: "RegisterDayOffEntities");

            migrationBuilder.DropColumn(
                name: "DayOffType",
                table: "DayOffConfigEntities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DayOffType",
                table: "RegisterDayOffEntities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DayOffType",
                table: "DayOffConfigEntities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
