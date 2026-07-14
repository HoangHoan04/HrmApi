using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.CreateTable(
                name: "CompanyEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    TaxCode = table.Column<string>(type: "text", nullable: true),
                    Hotline = table.Column<string>(type: "text", nullable: true),
                    PrefixMaleCode = table.Column<string>(type: "text", nullable: true),
                    PrefixFemaleCode = table.Column<string>(type: "text", nullable: true),
                    PrefixFullTimeCode = table.Column<string>(type: "text", nullable: true),
                    PrefixPartTimeCode = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DayComputeSalary = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsComputePrevMonth = table.Column<bool>(type: "boolean", nullable: true),
                    TimeKeepingStandardId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyEntities_CompanyEntities_CompanyEntityId",
                        column: x => x.CompanyEntityId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BranchEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    GroupSalary = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsUsingHrm = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchEntities_CompanyEntities_CompanyEntityId",
                        column: x => x.CompanyEntityId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DepartmentEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Limit = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsNotifyMarketing = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    BranchEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentEntities_BranchEntities_BranchEntityId",
                        column: x => x.BranchEntityId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartMasterEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    BranchEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartMasterEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartMasterEntities_BranchEntities_BranchEntityId",
                        column: x => x.BranchEntityId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartMasterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartMasterEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartEntities_PartMasterEntities_PartMasterEntityId",
                        column: x => x.PartMasterEntityId,
                        principalTable: "PartMasterEntities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PositionMasterEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsLimitHoursWorking = table.Column<bool>(type: "boolean", nullable: false),
                    Limit = table.Column<string>(type: "text", nullable: true),
                    WorkingHour = table.Column<int>(type: "integer", nullable: true),
                    IsTimeKeeping = table.Column<bool>(type: "boolean", nullable: false),
                    HourWorkingStart = table.Column<TimeSpan>(type: "interval", nullable: true),
                    HourWorkingEnd = table.Column<TimeSpan>(type: "interval", nullable: true),
                    HourSnapShotStart = table.Column<TimeSpan>(type: "interval", nullable: true),
                    HourSnapShotEnd = table.Column<TimeSpan>(type: "interval", nullable: true),
                    MinimumWorkingHour = table.Column<int>(type: "integer", nullable: true),
                    IsSwapPosition = table.Column<bool>(type: "boolean", nullable: false),
                    TargetChangePositionIds = table.Column<string>(type: "text", nullable: true),
                    IsApprovedWhenHiringCandidate = table.Column<bool>(type: "boolean", nullable: false),
                    IsHadASecondInterview = table.Column<bool>(type: "boolean", nullable: false),
                    IsApprovedDayOff = table.Column<bool>(type: "boolean", nullable: false),
                    IsAllowOverTimekeepingStandard = table.Column<bool>(type: "boolean", nullable: false),
                    QuantityStandard = table.Column<int>(type: "integer", nullable: true),
                    PartMasterEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionMasterEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionMasterEntities_PartMasterEntities_PartMasterEntityId",
                        column: x => x.PartMasterEntityId,
                        principalTable: "PartMasterEntities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PositionEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionMasterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionMasterEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionEntities_PositionMasterEntities_PositionMasterEntit~",
                        column: x => x.PositionMasterEntityId,
                        principalTable: "PositionMasterEntities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchEntities_CompanyEntityId",
                table: "BranchEntities",
                column: "CompanyEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEntities_CompanyEntityId",
                table: "CompanyEntities",
                column: "CompanyEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_BranchEntityId",
                table: "DepartmentEntities",
                column: "BranchEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PartEntities_PartMasterEntityId",
                table: "PartEntities",
                column: "PartMasterEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PartMasterEntities_BranchEntityId",
                table: "PartMasterEntities",
                column: "BranchEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntities_PositionMasterEntityId",
                table: "PositionEntities",
                column: "PositionMasterEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionMasterEntities_PartMasterEntityId",
                table: "PositionMasterEntities",
                column: "PartMasterEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentEntities");

            migrationBuilder.DropTable(
                name: "PartEntities");

            migrationBuilder.DropTable(
                name: "PositionEntities");

            migrationBuilder.DropTable(
                name: "PositionMasterEntities");

            migrationBuilder.DropTable(
                name: "PartMasterEntities");

            migrationBuilder.DropTable(
                name: "BranchEntities");

            migrationBuilder.DropTable(
                name: "CompanyEntities");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);
        }
    }
}
