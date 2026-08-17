using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Drop obsolete OPERATE_PERFORMANCE/DISCIPLINE and RECRUITMENT_TRAINING codes.
    /// System role packs are managed manually via Admin/API.
    /// </remarks>
    public partial class ReseedDisciplinePerformanceTrainingPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "RolePermissionEntities"
                WHERE "PermissionCode" IN (
                    'OPERATE_PERFORMANCE_VIEW',
                    'OPERATE_PERFORMANCE_MANAGE',
                    'OPERATE_PERFORMANCE_REVIEW_VIEW',
                    'OPERATE_DISCIPLINE_VIEW',
                    'OPERATE_DISCIPLINE_MANAGE',
                    'OPERATE_VIOLATION_VIEW',
                    'RECRUITMENT_TRAINING_VIEW',
                    'RECRUITMENT_TRAINING_MANAGE'
                )
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
