using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteWebAdminWave4Security : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleSubject",
                table: "UserEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftSubject",
                table: "UserEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "UserEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFactorEnabledAt",
                table: "UserEntities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecret",
                table: "UserEntities",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IpAllowlistEntryEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CidrOrIp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_IpAllowlistEntryEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmsGatewayConfigEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ApiUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SenderId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_SmsGatewayConfigEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZaloOaConfigEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OaId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    AppId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SecretKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ZaloOaConfigEntities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IpAllowlistEntryEntities_CidrOrIp",
                table: "IpAllowlistEntryEntities",
                column: "CidrOrIp");

            migrationBuilder.CreateIndex(
                name: "IX_SmsGatewayConfigEntities_Provider",
                table: "SmsGatewayConfigEntities",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_ZaloOaConfigEntities_OaId",
                table: "ZaloOaConfigEntities",
                column: "OaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IpAllowlistEntryEntities");

            migrationBuilder.DropTable(
                name: "SmsGatewayConfigEntities");

            migrationBuilder.DropTable(
                name: "ZaloOaConfigEntities");

            migrationBuilder.DropColumn(
                name: "GoogleSubject",
                table: "UserEntities");

            migrationBuilder.DropColumn(
                name: "MicrosoftSubject",
                table: "UserEntities");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "UserEntities");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabledAt",
                table: "UserEntities");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecret",
                table: "UserEntities");
        }
    }
}
