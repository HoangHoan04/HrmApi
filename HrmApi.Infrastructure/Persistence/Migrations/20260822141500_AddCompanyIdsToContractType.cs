using System;
using System.Collections.Generic;
using HrmApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260822141500_AddCompanyIdsToContractType")]
    public partial class AddCompanyIdsToContractType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "CompanyIds",
                table: "ContractTypeEntities",
                type: "uuid[]",
                nullable: false,
                defaultValue: new List<Guid>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyIds",
                table: "ContractTypeEntities");
        }
    }
}
