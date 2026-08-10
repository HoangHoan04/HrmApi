using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDatabaseOptimnize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractTypeEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsProbation = table.Column<bool>(type: "boolean", nullable: false),
                    IsUnlimited = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultDurationMonths = table.Column<int>(type: "integer", nullable: true),
                    MaxRenewalTimes = table.Column<int>(type: "integer", nullable: true),
                    NotifyBeforeExpiryDays = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_ContractTypeEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractTypeEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferEmployeeEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    TransferType = table.Column<string>(type: "text", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    DecisionNumber = table.Column<string>(type: "text", nullable: true),
                    DecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionFileUrl = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TransferEmployeeEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferEmployeeEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: false),
                    SignDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JobTitle = table.Column<string>(type: "text", nullable: true),
                    WorkingLocation = table.Column<string>(type: "text", nullable: true),
                    BasicSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    Allowance = table.Column<decimal>(type: "numeric", nullable: true),
                    InsuranceSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    PaymentMethod = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignedByCompanyRepresentative = table.Column<string>(type: "text", nullable: true),
                    SignedByEmployeeName = table.Column<string>(type: "text", nullable: true),
                    IsAutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    RenewalTimes = table.Column<int>(type: "integer", nullable: false),
                    TerminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminationReason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ContractEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractEntities_ContractEntities_PreviousContractId",
                        column: x => x.PreviousContractId,
                        principalTable: "ContractEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractEntities_ContractTypeEntities_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalTable: "ContractTypeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractEntities_DepartmentEntities_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DepartmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractEntities_PositionEntities_PositionId",
                        column: x => x.PositionId,
                        principalTable: "PositionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferEmployeePositionEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldCompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewCompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangeType = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TransferEmployeePositionEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_BranchEntities_NewBranchId",
                        column: x => x.NewBranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_BranchEntities_OldBranchId",
                        column: x => x.OldBranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_CompanyEntities_NewCompany~",
                        column: x => x.NewCompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_CompanyEntities_OldCompany~",
                        column: x => x.OldCompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_DepartmentEntities_NewDepa~",
                        column: x => x.NewDepartmentId,
                        principalTable: "DepartmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_DepartmentEntities_OldDepa~",
                        column: x => x.OldDepartmentId,
                        principalTable: "DepartmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_PartEntities_NewPartId",
                        column: x => x.NewPartId,
                        principalTable: "PartEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_PartEntities_OldPartId",
                        column: x => x.OldPartId,
                        principalTable: "PartEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_PositionEntities_NewPositi~",
                        column: x => x.NewPositionId,
                        principalTable: "PositionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_PositionEntities_OldPositi~",
                        column: x => x.OldPositionId,
                        principalTable: "PositionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferEmployeePositionEntities_TransferEmployeeEntities_T~",
                        column: x => x.TransferEmployeeId,
                        principalTable: "TransferEmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewRenewalEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "text", nullable: true),
                    PerformanceScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ReviewResult = table.Column<string>(type: "text", nullable: true),
                    ReviewComment = table.Column<string>(type: "text", nullable: true),
                    Recommendation = table.Column<string>(type: "text", nullable: true),
                    ProposedContractTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProposedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProposedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProposedBasicSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    NewContractId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ReviewRenewalEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewRenewalEntities_ContractEntities_ContractId",
                        column: x => x.ContractId,
                        principalTable: "ContractEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewRenewalEntities_ContractEntities_NewContractId",
                        column: x => x.NewContractId,
                        principalTable: "ContractEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewRenewalEntities_ContractTypeEntities_ProposedContract~",
                        column: x => x.ProposedContractTypeId,
                        principalTable: "ContractTypeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewRenewalEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_BranchId",
                table: "ContractEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_Code",
                table: "ContractEntities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_CompanyId",
                table: "ContractEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_ContractTypeId",
                table: "ContractEntities",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_DepartmentId",
                table: "ContractEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_EmployeeId_Status",
                table: "ContractEntities",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_EndDate",
                table: "ContractEntities",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_PositionId",
                table: "ContractEntities",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_PreviousContractId",
                table: "ContractEntities",
                column: "PreviousContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypeEntities_Code",
                table: "ContractTypeEntities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypeEntities_CompanyId",
                table: "ContractTypeEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRenewalEntities_ContractId_Status",
                table: "ReviewRenewalEntities",
                columns: new[] { "ContractId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRenewalEntities_EmployeeId",
                table: "ReviewRenewalEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRenewalEntities_NewContractId",
                table: "ReviewRenewalEntities",
                column: "NewContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRenewalEntities_ProposedContractTypeId",
                table: "ReviewRenewalEntities",
                column: "ProposedContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeeEntities_EmployeeId_EffectiveDate",
                table: "TransferEmployeeEntities",
                columns: new[] { "EmployeeId", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_EmployeeId",
                table: "TransferEmployeePositionEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_NewBranchId",
                table: "TransferEmployeePositionEntities",
                column: "NewBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_NewCompanyId",
                table: "TransferEmployeePositionEntities",
                column: "NewCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_NewDepartmentId",
                table: "TransferEmployeePositionEntities",
                column: "NewDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_NewPartId",
                table: "TransferEmployeePositionEntities",
                column: "NewPartId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_NewPositionId",
                table: "TransferEmployeePositionEntities",
                column: "NewPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_OldBranchId",
                table: "TransferEmployeePositionEntities",
                column: "OldBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_OldCompanyId",
                table: "TransferEmployeePositionEntities",
                column: "OldCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_OldDepartmentId",
                table: "TransferEmployeePositionEntities",
                column: "OldDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_OldPartId",
                table: "TransferEmployeePositionEntities",
                column: "OldPartId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_OldPositionId",
                table: "TransferEmployeePositionEntities",
                column: "OldPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferEmployeePositionEntities_TransferEmployeeId",
                table: "TransferEmployeePositionEntities",
                column: "TransferEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewRenewalEntities");

            migrationBuilder.DropTable(
                name: "TransferEmployeePositionEntities");

            migrationBuilder.DropTable(
                name: "ContractEntities");

            migrationBuilder.DropTable(
                name: "TransferEmployeeEntities");

            migrationBuilder.DropTable(
                name: "ContractTypeEntities");
        }
    }
}
