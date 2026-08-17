using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteTimeAttendanceP1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalNightMinutes",
                table: "TimekeepingSummaryEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalOtMinutes",
                table: "TimekeepingSummaryEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "NightEndTime",
                table: "TimeKeepingStandardEntities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 6, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "NightStartTime",
                table: "TimeKeepingStandardEntities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 22, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "NightMinutes",
                table: "TimekeepingEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OtMinutes",
                table: "TimekeepingEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OvertimeRequestEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FromTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ToTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    RequestedMinutes = table.Column<int>(type: "integer", nullable: false),
                    ApprovedMinutes = table.Column<int>(type: "integer", nullable: true),
                    OtType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApproverNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeRequestEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeRequestEntities_EmployeeEntities_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OvertimeRequestEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequestEntities_ApproverId",
                table: "OvertimeRequestEntities",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequestEntities_Code",
                table: "OvertimeRequestEntities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequestEntities_EmployeeId_WorkDate_Status",
                table: "OvertimeRequestEntities",
                columns: new[] { "EmployeeId", "WorkDate", "Status" });

            migrationBuilder.Sql("""
                UPDATE "TimeKeepingStandardEntities"
                SET "NightStartTime" = INTERVAL '22 hours',
                    "NightEndTime" = INTERVAL '6 hours'
                WHERE "NightStartTime" = INTERVAL '0' AND "NightEndTime" = INTERVAL '0';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OvertimeRequestEntities");

            migrationBuilder.DropColumn(
                name: "TotalNightMinutes",
                table: "TimekeepingSummaryEntities");

            migrationBuilder.DropColumn(
                name: "TotalOtMinutes",
                table: "TimekeepingSummaryEntities");

            migrationBuilder.DropColumn(
                name: "NightEndTime",
                table: "TimeKeepingStandardEntities");

            migrationBuilder.DropColumn(
                name: "NightStartTime",
                table: "TimeKeepingStandardEntities");

            migrationBuilder.DropColumn(
                name: "NightMinutes",
                table: "TimekeepingEntities");

            migrationBuilder.DropColumn(
                name: "OtMinutes",
                table: "TimekeepingEntities");
        }
    }
}
