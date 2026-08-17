using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>Drop obsolete training codes under recruitment. Role packs managed manually.</remarks>
    public partial class ReseedRecruitmentPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "RolePermissionEntities"
                WHERE "PermissionCode" IN ('RECRUITMENT_TRAINING_VIEW', 'RECRUITMENT_TRAINING_MANAGE')
                  AND "IsDeleted" = FALSE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only; irreversible.
        }
    }
}
