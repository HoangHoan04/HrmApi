using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdvanceEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    DeductMonth = table.Column<int>(type: "integer", nullable: true),
                    DeductYear = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AdvanceEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvanceEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllowanceEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false),
                    IsInsurable = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AllowanceEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowanceEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashAdditionSlipEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    AdditionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdditionType = table.Column<string>(type: "text", nullable: true),
                    ApplyMonth = table.Column<int>(type: "integer", nullable: true),
                    ApplyYear = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashAdditionSlipEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashAdditionSlipEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeductionSlipEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    DeductionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeductionType = table.Column<string>(type: "text", nullable: true),
                    ApplyMonth = table.Column<int>(type: "integer", nullable: true),
                    ApplyYear = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeductionSlipEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeductionSlipEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalaryCoefficientEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<string>(type: "text", nullable: true),
                    Coefficient = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("PK_SalaryCoefficientEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryCoefficientEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryConfigEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    StandardWorkingDays = table.Column<int>(type: "integer", nullable: false),
                    BhxhEmployeeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    BhytEmployeeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    BhtnEmployeeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    DefaultPayDay = table.Column<int>(type: "integer", nullable: true),
                    IsComputePrevMonth = table.Column<bool>(type: "boolean", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_SalaryConfigEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryConfigEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryIncreaseEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldBasicSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    NewBasicSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecisionNumber = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_SalaryIncreaseEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryIncreaseEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalaryEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryConfigId = table.Column<Guid>(type: "uuid", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    PeriodCode = table.Column<string>(type: "text", nullable: false),
                    PayDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    StandardWorkingDays = table.Column<decimal>(type: "numeric", nullable: true),
                    ActualWorkingDays = table.Column<decimal>(type: "numeric", nullable: true),
                    BasicSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalDeduction = table.Column<decimal>(type: "numeric", nullable: false),
                    NetSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    InsuranceSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    PayslipFileUrl = table.Column<string>(type: "text", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_SalaryEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryEntities_DepartmentEntities_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DepartmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalaryEntities_PositionEntities_PositionId",
                        column: x => x.PositionId,
                        principalTable: "PositionEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryEntities_SalaryConfigEntities_SalaryConfigId",
                        column: x => x.SalaryConfigId,
                        principalTable: "SalaryConfigEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryLineItemEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "text", nullable: false),
                    ItemCode = table.Column<string>(type: "text", nullable: false),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_SalaryLineItemEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryLineItemEntities_SalaryEntities_SalaryId",
                        column: x => x.SalaryId,
                        principalTable: "SalaryEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceEntities_EmployeeId_Status",
                table: "AdvanceEntities",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AllowanceEntities_Code",
                table: "AllowanceEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_AllowanceEntities_CompanyId",
                table: "AllowanceEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAdditionSlipEntities_EmployeeId",
                table: "CashAdditionSlipEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeductionSlipEntities_EmployeeId",
                table: "DeductionSlipEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCoefficientEntities_Code",
                table: "SalaryCoefficientEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCoefficientEntities_CompanyId",
                table: "SalaryCoefficientEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryConfigEntities_Code",
                table: "SalaryConfigEntities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryConfigEntities_CompanyId",
                table: "SalaryConfigEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryEntities_BranchId",
                table: "SalaryEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryEntities_CompanyId",
                table: "SalaryEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryEntities_DepartmentId",
                table: "SalaryEntities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryEntities_EmployeeId_Year_Month",
                table: "SalaryEntities",
                columns: new[] { "EmployeeId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryEntities_PeriodCode",
                table: "SalaryEntities",
                column: "PeriodCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryEntities_PositionId",
                table: "SalaryEntities",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryEntities_SalaryConfigId",
                table: "SalaryEntities",
                column: "SalaryConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryEntities_Status",
                table: "SalaryEntities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryIncreaseEntities_EmployeeId",
                table: "SalaryIncreaseEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryLineItemEntities_SalaryId_ItemCode",
                table: "SalaryLineItemEntities",
                columns: new[] { "SalaryId", "ItemCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvanceEntities");

            migrationBuilder.DropTable(
                name: "AllowanceEntities");

            migrationBuilder.DropTable(
                name: "CashAdditionSlipEntities");

            migrationBuilder.DropTable(
                name: "DeductionSlipEntities");

            migrationBuilder.DropTable(
                name: "SalaryCoefficientEntities");

            migrationBuilder.DropTable(
                name: "SalaryIncreaseEntities");

            migrationBuilder.DropTable(
                name: "SalaryLineItemEntities");

            migrationBuilder.DropTable(
                name: "SalaryEntities");

            migrationBuilder.DropTable(
                name: "SalaryConfigEntities");
        }
    }
}
