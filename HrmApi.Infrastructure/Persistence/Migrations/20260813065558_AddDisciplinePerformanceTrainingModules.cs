using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDisciplinePerformanceTrainingModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetencyFrameworkEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_CompetencyFrameworkEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetencyFrameworkEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReviewCycleEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReviewCycleEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceReviewCycleEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformanceReviewCycleEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCourseEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "text", nullable: true),
                    Hours = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCourseEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingCourseEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingCourseEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ViolationTypeEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationTypeEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiGoalEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiGoalEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiGoalEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KpiGoalEntities_PerformanceReviewCycleEntities_CycleId",
                        column: x => x.CycleId,
                        principalTable: "PerformanceReviewCycleEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingEnrollmentEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingEnrollmentEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingEnrollmentEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingEnrollmentEntities_TrainingCourseEntities_CourseId",
                        column: x => x.CourseId,
                        principalTable: "TrainingCourseEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViolationEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    ViolationTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Decision = table.Column<string>(type: "text", nullable: true),
                    PenaltyType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViolationEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViolationEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViolationEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViolationEntities_ViolationTypeEntities_ViolationTypeId",
                        column: x => x.ViolationTypeId,
                        principalTable: "ViolationTypeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KpiResultEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    RatedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiResultEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiResultEntities_EmployeeEntities_RatedByEmployeeId",
                        column: x => x.RatedByEmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KpiResultEntities_KpiGoalEntities_GoalId",
                        column: x => x.GoalId,
                        principalTable: "KpiGoalEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingResultEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Grade = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificateUrl = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingResultEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingResultEntities_TrainingEnrollmentEntities_Enrollmen~",
                        column: x => x.EnrollmentId,
                        principalTable: "TrainingEnrollmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyFrameworkEntities_Code",
                table: "CompetencyFrameworkEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyFrameworkEntities_CompanyId",
                table: "CompetencyFrameworkEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiGoalEntities_CycleId_EmployeeId",
                table: "KpiGoalEntities",
                columns: new[] { "CycleId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_KpiGoalEntities_EmployeeId",
                table: "KpiGoalEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiResultEntities_GoalId",
                table: "KpiResultEntities",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiResultEntities_RatedByEmployeeId",
                table: "KpiResultEntities",
                column: "RatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviewCycleEntities_BranchId",
                table: "PerformanceReviewCycleEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviewCycleEntities_Code",
                table: "PerformanceReviewCycleEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviewCycleEntities_CompanyId",
                table: "PerformanceReviewCycleEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviewCycleEntities_Status_CompanyId",
                table: "PerformanceReviewCycleEntities",
                columns: new[] { "Status", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseEntities_BranchId",
                table: "TrainingCourseEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseEntities_Code",
                table: "TrainingCourseEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseEntities_CompanyId",
                table: "TrainingCourseEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseEntities_Status_CompanyId",
                table: "TrainingCourseEntities",
                columns: new[] { "Status", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingEnrollmentEntities_CourseId_EmployeeId",
                table: "TrainingEnrollmentEntities",
                columns: new[] { "CourseId", "EmployeeId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingEnrollmentEntities_EmployeeId",
                table: "TrainingEnrollmentEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingResultEntities_EnrollmentId",
                table: "TrainingResultEntities",
                column: "EnrollmentId",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEntities_BranchId",
                table: "ViolationEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEntities_Code",
                table: "ViolationEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEntities_CompanyId",
                table: "ViolationEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEntities_EmployeeId",
                table: "ViolationEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEntities_Status_CompanyId",
                table: "ViolationEntities",
                columns: new[] { "Status", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEntities_ViolationTypeId",
                table: "ViolationEntities",
                column: "ViolationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationTypeEntities_Code",
                table: "ViolationTypeEntities",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetencyFrameworkEntities");

            migrationBuilder.DropTable(
                name: "KpiResultEntities");

            migrationBuilder.DropTable(
                name: "TrainingResultEntities");

            migrationBuilder.DropTable(
                name: "ViolationEntities");

            migrationBuilder.DropTable(
                name: "KpiGoalEntities");

            migrationBuilder.DropTable(
                name: "TrainingEnrollmentEntities");

            migrationBuilder.DropTable(
                name: "ViolationTypeEntities");

            migrationBuilder.DropTable(
                name: "PerformanceReviewCycleEntities");

            migrationBuilder.DropTable(
                name: "TrainingCourseEntities");
        }
    }
}
