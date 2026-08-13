using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeAuthWaveBIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserTokenEntities_RefreshTokenHash",
                table: "UserTokenEntities",
                column: "RefreshTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_Username",
                table: "UserEntities",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTokenEntities_RefreshTokenHash",
                table: "UserTokenEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserEntities_Username",
                table: "UserEntities");
        }
    }
}
