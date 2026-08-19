using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetsInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSerialRequired",
                table: "AssetTypeEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxPerEmployee",
                table: "AssetTypeEntities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "AssetTicketEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "AssetTicketEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReturnExpectedDate",
                table: "AssetTicketEntities",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ToEmployeeId",
                table: "AssetTicketEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AssetEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "AssetEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vendor",
                table: "AssetEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "WarrantyExpiryDate",
                table: "AssetEntities",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetAssignmentEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuedTicketId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReturnedTicketId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConditionOnIssue = table.Column<string>(type: "text", nullable: true),
                    ConditionOnReturn = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AssetAssignmentEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAssignmentEntities_AssetEntities_AssetId",
                        column: x => x.AssetId,
                        principalTable: "AssetEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetAssignmentEntities_AssetTicketEntities_IssuedTicketId",
                        column: x => x.IssuedTicketId,
                        principalTable: "AssetTicketEntities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetAssignmentEntities_AssetTicketEntities_ReturnedTicketId",
                        column: x => x.ReturnedTicketId,
                        principalTable: "AssetTicketEntities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetAssignmentEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetAssignmentEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetAssignmentEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetTicketEntities_BranchId",
                table: "AssetTicketEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTicketEntities_ToEmployeeId",
                table: "AssetTicketEntities",
                column: "ToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignmentEntities_AssetId",
                table: "AssetAssignmentEntities",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignmentEntities_BranchId",
                table: "AssetAssignmentEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignmentEntities_CompanyId",
                table: "AssetAssignmentEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignmentEntities_EmployeeId",
                table: "AssetAssignmentEntities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignmentEntities_IssuedTicketId",
                table: "AssetAssignmentEntities",
                column: "IssuedTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignmentEntities_ReturnedTicketId",
                table: "AssetAssignmentEntities",
                column: "ReturnedTicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetTicketEntities_BranchEntities_BranchId",
                table: "AssetTicketEntities",
                column: "BranchId",
                principalTable: "BranchEntities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetTicketEntities_EmployeeEntities_ToEmployeeId",
                table: "AssetTicketEntities",
                column: "ToEmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetTicketEntities_BranchEntities_BranchId",
                table: "AssetTicketEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetTicketEntities_EmployeeEntities_ToEmployeeId",
                table: "AssetTicketEntities");

            migrationBuilder.DropTable(
                name: "AssetAssignmentEntities");

            migrationBuilder.DropIndex(
                name: "IX_AssetTicketEntities_BranchId",
                table: "AssetTicketEntities");

            migrationBuilder.DropIndex(
                name: "IX_AssetTicketEntities_ToEmployeeId",
                table: "AssetTicketEntities");

            migrationBuilder.DropColumn(
                name: "IsSerialRequired",
                table: "AssetTypeEntities");

            migrationBuilder.DropColumn(
                name: "MaxPerEmployee",
                table: "AssetTypeEntities");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AssetTicketEntities");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "AssetTicketEntities");

            migrationBuilder.DropColumn(
                name: "ReturnExpectedDate",
                table: "AssetTicketEntities");

            migrationBuilder.DropColumn(
                name: "ToEmployeeId",
                table: "AssetTicketEntities");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AssetEntities");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "AssetEntities");

            migrationBuilder.DropColumn(
                name: "Vendor",
                table: "AssetEntities");

            migrationBuilder.DropColumn(
                name: "WarrantyExpiryDate",
                table: "AssetEntities");
        }
    }
}
