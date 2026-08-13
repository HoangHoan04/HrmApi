using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteWebAdminWave1Domain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BudgetAmount",
                table: "TrainingCourseEntities",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetTypeEntities",
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
                    table.PrimaryKey("PK_AssetTypeEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTypeEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Performance360ReviewEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerType = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Performance360ReviewEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Performance360ReviewEntities_EmployeeEntities_ReviewerEmplo~",
                        column: x => x.ReviewerEmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Performance360ReviewEntities_EmployeeEntities_SubjectEmploy~",
                        column: x => x.SubjectEmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Performance360ReviewEntities_PerformanceReviewCycleEntities~",
                        column: x => x.CycleId,
                        principalTable: "PerformanceReviewCycleEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCourseMaterialEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_TrainingCourseMaterialEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingCourseMaterialEntities_TrainingCourseEntities_Cours~",
                        column: x => x.CourseId,
                        principalTable: "TrainingCourseEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingQuizEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    OptionA = table.Column<string>(type: "text", nullable: false),
                    OptionB = table.Column<string>(type: "text", nullable: false),
                    OptionC = table.Column<string>(type: "text", nullable: true),
                    OptionD = table.Column<string>(type: "text", nullable: true),
                    CorrectOption = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingQuizEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingQuizEntities_TrainingCourseEntities_CourseId",
                        column: x => x.CourseId,
                        principalTable: "TrainingCourseEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AssetTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PurchaseCost = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("PK_AssetEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetEntities_AssetTypeEntities_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "AssetTypeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetTicketEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TicketAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AssetTicketEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTicketEntities_AssetEntities_AssetId",
                        column: x => x.AssetId,
                        principalTable: "AssetEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetTicketEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTicketEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetEntities_AssetTypeId",
                table: "AssetEntities",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEntities_BranchId",
                table: "AssetEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEntities_Code",
                table: "AssetEntities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetEntities_CompanyId",
                table: "AssetEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEntities_Status_CompanyId",
                table: "AssetEntities",
                columns: new[] { "Status", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetTicketEntities_AssetId",
                table: "AssetTicketEntities",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTicketEntities_Code",
                table: "AssetTicketEntities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetTicketEntities_CompanyId",
                table: "AssetTicketEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTicketEntities_EmployeeId",
                table: "AssetTicketEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTicketEntities_Status_CompanyId",
                table: "AssetTicketEntities",
                columns: new[] { "Status", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetTypeEntities_Code",
                table: "AssetTypeEntities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetTypeEntities_CompanyId",
                table: "AssetTypeEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Performance360ReviewEntities_CycleId",
                table: "Performance360ReviewEntities",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Performance360ReviewEntities_ReviewerEmployeeId",
                table: "Performance360ReviewEntities",
                column: "ReviewerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Performance360ReviewEntities_SubjectEmployeeId",
                table: "Performance360ReviewEntities",
                column: "SubjectEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseMaterialEntities_CourseId_DisplayOrder",
                table: "TrainingCourseMaterialEntities",
                columns: new[] { "CourseId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuizEntities_CourseId",
                table: "TrainingQuizEntities",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetTicketEntities");

            migrationBuilder.DropTable(
                name: "Performance360ReviewEntities");

            migrationBuilder.DropTable(
                name: "TrainingCourseMaterialEntities");

            migrationBuilder.DropTable(
                name: "TrainingQuizEntities");

            migrationBuilder.DropTable(
                name: "AssetEntities");

            migrationBuilder.DropTable(
                name: "AssetTypeEntities");

            migrationBuilder.DropColumn(
                name: "BudgetAmount",
                table: "TrainingCourseEntities");
        }
    }
}
