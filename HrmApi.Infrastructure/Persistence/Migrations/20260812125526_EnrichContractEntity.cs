using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichContractEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<int>(
                name: "AnnualLeaveDays",
                table: "ContractEntities",
                type: "integer",
                nullable: true);

            _ = migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ContractEntities",
                type: "text",
                nullable: true);

            _ = migrationBuilder.AddColumn<string>(
                name: "DecisionNumber",
                table: "ContractEntities",
                type: "text",
                nullable: true);

            _ = migrationBuilder.AddColumn<string>(
                name: "JobDescription",
                table: "ContractEntities",
                type: "text",
                nullable: true);

            _ = migrationBuilder.AddColumn<Guid>(
                name: "PartId",
                table: "ContractEntities",
                type: "uuid",
                nullable: true);

            _ = migrationBuilder.AddColumn<DateTime>(
                name: "ProbationEndDate",
                table: "ContractEntities",
                type: "timestamp with time zone",
                nullable: true);

            _ = migrationBuilder.AddColumn<decimal>(
                name: "SalaryCoefficient",
                table: "ContractEntities",
                type: "numeric",
                nullable: true);

            _ = migrationBuilder.AddColumn<decimal>(
                name: "WorkingHoursPerWeek",
                table: "ContractEntities",
                type: "numeric",
                nullable: true);

            _ = migrationBuilder.AddColumn<string>(
                name: "WorkingMode",
                table: "ContractEntities",
                type: "text",
                nullable: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_ContractEntities_PartId",
                table: "ContractEntities",
                column: "PartId");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_ContractEntities_PartEntities_PartId",
                table: "ContractEntities",
                column: "PartId",
                principalTable: "PartEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(
                name: "FK_ContractEntities_PartEntities_PartId",
                table: "ContractEntities");

            _ = migrationBuilder.DropIndex(
                name: "IX_ContractEntities_PartId",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "AnnualLeaveDays",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "Currency",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "DecisionNumber",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "JobDescription",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "PartId",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "ProbationEndDate",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "SalaryCoefficient",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "WorkingHoursPerWeek",
                table: "ContractEntities");

            _ = migrationBuilder.DropColumn(
                name: "WorkingMode",
                table: "ContractEntities");
        }
    }
}
