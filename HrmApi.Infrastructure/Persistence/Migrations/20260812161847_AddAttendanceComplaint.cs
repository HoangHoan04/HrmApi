using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceComplaint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceComplaintEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimekeepingId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComplaintType = table.Column<string>(type: "text", nullable: false),
                    RequestedCheckInTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    RequestedCheckOutTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApproverNote = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceComplaintEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceComplaintEntities_EmployeeEntities_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceComplaintEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceComplaintEntities_TimekeepingEntities_Timekeeping~",
                        column: x => x.TimekeepingId,
                        principalTable: "TimekeepingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceComplaintEntities_ApproverId",
                table: "AttendanceComplaintEntities",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceComplaintEntities_EmployeeId_WorkDate_Status",
                table: "AttendanceComplaintEntities",
                columns: new[] { "EmployeeId", "WorkDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceComplaintEntities_TimekeepingId",
                table: "AttendanceComplaintEntities",
                column: "TimekeepingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceComplaintEntities");
        }
    }
}
