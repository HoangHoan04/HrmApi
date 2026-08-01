using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeOrrganizition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PositionMasterEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PositionMasterEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PositionEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PositionEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QuantityStandard",
                table: "PositionEntities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PartMasterEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PartMasterEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PartEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PartEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PartEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PartEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Limit",
                table: "PartEntities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "PartEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PartEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostCenterCode",
                table: "DepartmentEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentHeadCount",
                table: "DepartmentEntities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentEntityId",
                table: "DepartmentEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeputyManagerId",
                table: "DepartmentEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "DepartmentEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DissolvedDate",
                table: "DepartmentEntities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DepartmentEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstablishedDate",
                table: "DepartmentEntities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DepartmentEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "DepartmentEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "DepartmentEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentDepartmentId",
                table: "DepartmentEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneExtension",
                table: "DepartmentEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "DepartmentEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranch",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationCode",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyType",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultLanguage",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FoundedDate",
                table: "CompanyEntities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CompanyEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentative",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentativePosition",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingStatus",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialInsuranceCode",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "CompanyEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchEntityId",
                table: "BranchEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationCode",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosingDate",
                table: "BranchEntities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "BranchEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BranchEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHeadQuarter",
                table: "BranchEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "BranchEntities",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "BranchEntities",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "BranchEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerName",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerPhone",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxEmployeeCapacity",
                table: "BranchEntities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpeningDate",
                table: "BranchEntities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingStatus",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentBranchId",
                table: "BranchEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TimeKeepingStandardId",
                table: "BranchEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "BranchEntities",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentEntities_DepartmentEntityId",
                table: "DepartmentEntities",
                column: "DepartmentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchEntities_BranchEntityId",
                table: "BranchEntities",
                column: "BranchEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchEntities_BranchEntities_BranchEntityId",
                table: "BranchEntities",
                column: "BranchEntityId",
                principalTable: "BranchEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentEntities_DepartmentEntities_DepartmentEntityId",
                table: "DepartmentEntities",
                column: "DepartmentEntityId",
                principalTable: "DepartmentEntities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchEntities_BranchEntities_BranchEntityId",
                table: "BranchEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentEntities_DepartmentEntities_DepartmentEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentEntities_DepartmentEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_BranchEntities_BranchEntityId",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PositionMasterEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PositionMasterEntities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PositionEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PositionEntities");

            migrationBuilder.DropColumn(
                name: "QuantityStandard",
                table: "PositionEntities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PartMasterEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PartMasterEntities");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "Limit",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "PartEntities");

            migrationBuilder.DropColumn(
                name: "CostCenterCode",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "CurrentHeadCount",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "DepartmentEntityId",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "DeputyManagerId",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "DissolvedDate",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "EstablishedDate",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "ParentDepartmentId",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "PhoneExtension",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "DepartmentEntities");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "BankBranch",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationCode",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "City",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "CompanyType",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "DefaultLanguage",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "District",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "FoundedDate",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "LegalRepresentative",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "LegalRepresentativePosition",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "OperatingStatus",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "SocialInsuranceCode",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "CompanyEntities");

            migrationBuilder.DropColumn(
                name: "BranchEntityId",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationCode",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "City",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "ClosingDate",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "District",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "IsHeadQuarter",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "ManagerName",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "ManagerPhone",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "MaxEmployeeCapacity",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "OpeningDate",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "OperatingStatus",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "ParentBranchId",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "TimeKeepingStandardId",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "BranchEntities");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "BranchEntities");
        }
    }
}
