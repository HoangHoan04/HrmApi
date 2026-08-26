using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserTablesAndAddUserIdToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleEntities_UserEntities_UserId",
                table: "UserRoleEntities");

            migrationBuilder.DropTable(
                name: "UserTokenEntities");

            migrationBuilder.DropTable(
                name: "UserEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleEntities_UserId",
                table: "UserRoleEntities");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserRoleEntities",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "UserRoleEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "EmployeeEntities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleEntities_EmployeeId_RoleId",
                table: "UserRoleEntities",
                columns: new[] { "EmployeeId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleEntities_UserId_RoleId",
                table: "UserRoleEntities",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleEntities_EmployeeEntities_EmployeeId",
                table: "UserRoleEntities",
                column: "EmployeeId",
                principalTable: "EmployeeEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleEntities_EmployeeEntities_EmployeeId",
                table: "UserRoleEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleEntities_EmployeeId_RoleId",
                table: "UserRoleEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleEntities_UserId_RoleId",
                table: "UserRoleEntities");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "UserRoleEntities");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EmployeeEntities");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserRoleEntities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UserEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    FcmToken = table.Column<string>(type: "text", nullable: true),
                    FcmTokenMobile = table.Column<string>(type: "text", nullable: true),
                    GoogleSubject = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginIp = table.Column<string>(type: "text", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MicrosoftSubject = table.Column<string>(type: "text", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    ResetPasswordOtp = table.Column<string>(type: "text", nullable: true),
                    ResetPasswordOtpExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TwoFactorSecret = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserEntities_BranchEntities_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserEntities_CompanyEntities_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserEntities_EmployeeEntities_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTokenEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: true),
                    DeviceName = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "text", nullable: false),
                    ReplacedByTokenId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokenEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokenEntities_UserEntities_UserId",
                        column: x => x.UserId,
                        principalTable: "UserEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTokenEntities_UserTokenEntities_ReplacedByTokenId",
                        column: x => x.ReplacedByTokenId,
                        principalTable: "UserTokenEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleEntities_UserId",
                table: "UserRoleEntities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_BranchId",
                table: "UserEntities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_CompanyId",
                table: "UserEntities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_EmployeeId",
                table: "UserEntities",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_Username",
                table: "UserEntities",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokenEntities_RefreshTokenHash",
                table: "UserTokenEntities",
                column: "RefreshTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokenEntities_ReplacedByTokenId",
                table: "UserTokenEntities",
                column: "ReplacedByTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokenEntities_UserId",
                table: "UserTokenEntities",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleEntities_UserEntities_UserId",
                table: "UserRoleEntities",
                column: "UserId",
                principalTable: "UserEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
