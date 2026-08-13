using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluationCriteriaEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    DefaultWeight = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_EvaluationCriteriaEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationCriteriaEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HiringSourceEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_HiringSourceEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobDescriptionEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Responsibilities = table.Column<string>(type: "text", nullable: true),
                    Requirements = table.Column<string>(type: "text", nullable: true),
                    Benefits = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionMasterId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_JobDescriptionEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobDescriptionEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobDescriptionEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobDescriptionEntities_DepartmentEntities_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DepartmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobDescriptionEntities_PartEntities_PartId",
                        column: x => x.PartId,
                        principalTable: "PartEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobDescriptionEntities_PositionEntities_PositionId",
                        column: x => x.PositionId,
                        principalTable: "PositionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobDescriptionEntities_PositionMasterEntities_PositionMaste~",
                        column: x => x.PositionMasterId,
                        principalTable: "PositionMasterEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecruitmentRequestEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    RequestLevel = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobDescriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ExpectedStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNote = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitmentRequestEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestEntities_DepartmentEntities_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DepartmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestEntities_EmployeeEntities_ApprovedByEmplo~",
                        column: x => x.ApprovedByEmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestEntities_EmployeeEntities_RequestedByEmpl~",
                        column: x => x.RequestedByEmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestEntities_JobDescriptionEntities_JobDescri~",
                        column: x => x.JobDescriptionId,
                        principalTable: "JobDescriptionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestEntities_PartEntities_PartId",
                        column: x => x.PartId,
                        principalTable: "PartEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestEntities_PositionEntities_PositionId",
                        column: x => x.PositionId,
                        principalTable: "PositionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HiringPlanEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RecruitmentRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobDescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetQuantity = table.Column<int>(type: "integer", nullable: false),
                    OpenFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    OpenTo = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_HiringPlanEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiringPlanEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringPlanEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringPlanEntities_DepartmentEntities_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DepartmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringPlanEntities_JobDescriptionEntities_JobDescriptionId",
                        column: x => x.JobDescriptionId,
                        principalTable: "JobDescriptionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringPlanEntities_PartEntities_PartId",
                        column: x => x.PartId,
                        principalTable: "PartEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringPlanEntities_PositionEntities_PositionId",
                        column: x => x.PositionId,
                        principalTable: "PositionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringPlanEntities_RecruitmentRequestEntities_RecruitmentRe~",
                        column: x => x.RecruitmentRequestId,
                        principalTable: "RecruitmentRequestEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CandidateEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    CvUrl = table.Column<string>(type: "text", nullable: true),
                    HiringPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecruitmentRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    HiringSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateEntities_HiringPlanEntities_HiringPlanId",
                        column: x => x.HiringPlanId,
                        principalTable: "HiringPlanEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateEntities_HiringSourceEntities_HiringSourceId",
                        column: x => x.HiringSourceId,
                        principalTable: "HiringSourceEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateEntities_RecruitmentRequestEntities_RecruitmentReq~",
                        column: x => x.RecruitmentRequestId,
                        principalTable: "RecruitmentRequestEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HiringPlanCriteriaEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HiringPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationCriteriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("PK_HiringPlanCriteriaEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiringPlanCriteriaEntities_EvaluationCriteriaEntities_Evalu~",
                        column: x => x.EvaluationCriteriaId,
                        principalTable: "EvaluationCriteriaEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringPlanCriteriaEntities_HiringPlanEntities_HiringPlanId",
                        column: x => x.HiringPlanId,
                        principalTable: "HiringPlanEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewScheduleEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    HiringPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    MeetingUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewScheduleEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewScheduleEntities_CandidateEntities_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewScheduleEntities_HiringPlanEntities_HiringPlanId",
                        column: x => x.HiringPlanId,
                        principalTable: "HiringPlanEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterviewEvaluationEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationCriteriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewEvaluationEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewEvaluationEntities_EmployeeEntities_InterviewerEmp~",
                        column: x => x.InterviewerEmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewEvaluationEntities_EvaluationCriteriaEntities_Eval~",
                        column: x => x.EvaluationCriteriaId,
                        principalTable: "EvaluationCriteriaEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewEvaluationEntities_InterviewScheduleEntities_Inter~",
                        column: x => x.InterviewScheduleId,
                        principalTable: "InterviewScheduleEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewInterviewerEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewInterviewerEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewInterviewerEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewInterviewerEntities_InterviewScheduleEntities_Inte~",
                        column: x => x.InterviewScheduleId,
                        principalTable: "InterviewScheduleEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEntities_Code",
                table: "CandidateEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEntities_EmployeeId",
                table: "CandidateEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEntities_HiringPlanId",
                table: "CandidateEntities",
                column: "HiringPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEntities_HiringSourceId",
                table: "CandidateEntities",
                column: "HiringSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEntities_RecruitmentRequestId",
                table: "CandidateEntities",
                column: "RecruitmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEntities_Status_HiringPlanId",
                table: "CandidateEntities",
                columns: new[] { "Status", "HiringPlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationCriteriaEntities_Code",
                table: "EvaluationCriteriaEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationCriteriaEntities_CompanyId",
                table: "EvaluationCriteriaEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanCriteriaEntities_EvaluationCriteriaId",
                table: "HiringPlanCriteriaEntities",
                column: "EvaluationCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanCriteriaEntities_HiringPlanId_EvaluationCriteriaId",
                table: "HiringPlanCriteriaEntities",
                columns: new[] { "HiringPlanId", "EvaluationCriteriaId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_BranchId",
                table: "HiringPlanEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_Code",
                table: "HiringPlanEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_CompanyId",
                table: "HiringPlanEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_DepartmentId",
                table: "HiringPlanEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_JobDescriptionId",
                table: "HiringPlanEntities",
                column: "JobDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_PartId",
                table: "HiringPlanEntities",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_PositionId",
                table: "HiringPlanEntities",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_RecruitmentRequestId",
                table: "HiringPlanEntities",
                column: "RecruitmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringPlanEntities_Status_CompanyId",
                table: "HiringPlanEntities",
                columns: new[] { "Status", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_HiringSourceEntities_Code",
                table: "HiringSourceEntities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewEvaluationEntities_EvaluationCriteriaId",
                table: "InterviewEvaluationEntities",
                column: "EvaluationCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewEvaluationEntities_InterviewerEmployeeId",
                table: "InterviewEvaluationEntities",
                column: "InterviewerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewEvaluationEntities_InterviewScheduleId_Interviewer~",
                table: "InterviewEvaluationEntities",
                columns: new[] { "InterviewScheduleId", "InterviewerEmployeeId", "EvaluationCriteriaId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewInterviewerEntities_EmployeeId",
                table: "InterviewInterviewerEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewInterviewerEntities_InterviewScheduleId_EmployeeId",
                table: "InterviewInterviewerEntities",
                columns: new[] { "InterviewScheduleId", "EmployeeId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewScheduleEntities_CandidateId_Round",
                table: "InterviewScheduleEntities",
                columns: new[] { "CandidateId", "Round" });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewScheduleEntities_HiringPlanId",
                table: "InterviewScheduleEntities",
                column: "HiringPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewScheduleEntities_StartAt_EndAt",
                table: "InterviewScheduleEntities",
                columns: new[] { "StartAt", "EndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptionEntities_BranchId",
                table: "JobDescriptionEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptionEntities_Code",
                table: "JobDescriptionEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptionEntities_CompanyId",
                table: "JobDescriptionEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptionEntities_DepartmentId",
                table: "JobDescriptionEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptionEntities_PartId",
                table: "JobDescriptionEntities",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptionEntities_PositionId",
                table: "JobDescriptionEntities",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptionEntities_PositionMasterId",
                table: "JobDescriptionEntities",
                column: "PositionMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_ApprovedByEmployeeId",
                table: "RecruitmentRequestEntities",
                column: "ApprovedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_BranchId",
                table: "RecruitmentRequestEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_Code",
                table: "RecruitmentRequestEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_CompanyId",
                table: "RecruitmentRequestEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_DepartmentId",
                table: "RecruitmentRequestEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_JobDescriptionId",
                table: "RecruitmentRequestEntities",
                column: "JobDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_PartId",
                table: "RecruitmentRequestEntities",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_PositionId",
                table: "RecruitmentRequestEntities",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_RequestedByEmployeeId",
                table: "RecruitmentRequestEntities",
                column: "RequestedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestEntities_Status_CompanyId",
                table: "RecruitmentRequestEntities",
                columns: new[] { "Status", "CompanyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiringPlanCriteriaEntities");

            migrationBuilder.DropTable(
                name: "InterviewEvaluationEntities");

            migrationBuilder.DropTable(
                name: "InterviewInterviewerEntities");

            migrationBuilder.DropTable(
                name: "EvaluationCriteriaEntities");

            migrationBuilder.DropTable(
                name: "InterviewScheduleEntities");

            migrationBuilder.DropTable(
                name: "CandidateEntities");

            migrationBuilder.DropTable(
                name: "HiringPlanEntities");

            migrationBuilder.DropTable(
                name: "HiringSourceEntities");

            migrationBuilder.DropTable(
                name: "RecruitmentRequestEntities");

            migrationBuilder.DropTable(
                name: "JobDescriptionEntities");
        }
    }
}
