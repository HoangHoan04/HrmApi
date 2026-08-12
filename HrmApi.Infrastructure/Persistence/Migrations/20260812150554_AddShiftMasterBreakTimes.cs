using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftMasterBreakTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "BreakEndTime",
                table: "ShiftMasterEntities",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BreakStartTime",
                table: "ShiftMasterEntities",
                type: "interval",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakEndTime",
                table: "ShiftMasterEntities");

            migrationBuilder.DropColumn(
                name: "BreakStartTime",
                table: "ShiftMasterEntities");
        }
    }
}
