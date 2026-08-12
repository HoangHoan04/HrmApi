using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeWorkPattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeWorkPatternEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatternType = table.Column<string>(type: "text", nullable: false),
                    WorkOnMonday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkOnTuesday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkOnWednesday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkOnThursday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkOnFriday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkOnSaturday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkOnSunday = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWorkPatternEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkPatternEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkPatternEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkPatternEntities_ShiftMasterEntities_ShiftMaster~",
                        column: x => x.ShiftMasterId,
                        principalTable: "ShiftMasterEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkPatternEntities_BranchId",
                table: "EmployeeWorkPatternEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkPatternEntities_EmployeeId_EffectiveFrom_IsActi~",
                table: "EmployeeWorkPatternEntities",
                columns: new[] { "EmployeeId", "EffectiveFrom", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkPatternEntities_ShiftMasterId",
                table: "EmployeeWorkPatternEntities",
                column: "ShiftMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeWorkPatternEntities");
        }
    }
}
