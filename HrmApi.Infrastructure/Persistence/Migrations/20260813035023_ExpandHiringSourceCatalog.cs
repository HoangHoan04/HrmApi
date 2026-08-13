using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrmApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandHiringSourceCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChannelType",
                table: "HiringSourceEntities",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "HiringSourceEntities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "HiringSourceEntities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "HiringSourceEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "HiringSourceEntities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_HiringSourceEntities_DisplayOrder",
                table: "HiringSourceEntities",
                column: "DisplayOrder");

            // Backfill channel for rows created before Expand (EnsureSystem syncs order/catalog on next list call).
            migrationBuilder.Sql("""
                UPDATE "HiringSourceEntities"
                SET "ChannelType" = CASE UPPER("Code")
                    WHEN 'REFERRAL' THEN 'REFERRAL'
                    WHEN 'HR_EMAIL' THEN 'EMAIL'
                    WHEN 'CAREERS_SITE' THEN 'CAREERS_SITE'
                    WHEN 'FACEBOOK' THEN 'SOCIAL'
                    WHEN 'TOPCV' THEN 'JOBBOARD'
                    WHEN 'ITVIEC' THEN 'JOBBOARD'
                    WHEN 'LINKEDIN' THEN 'JOBBOARD'
                    WHEN 'JOBBOARD' THEN 'JOBBOARD'
                    WHEN 'HEADHUNTER' THEN 'AGENCY'
                    WHEN 'WALK_IN' THEN 'WALK_IN'
                    WHEN 'OTHER' THEN 'OTHER'
                    ELSE COALESCE(NULLIF("ChannelType", ''), 'OTHER')
                END,
                "IsSystem" = CASE WHEN UPPER("Code") IN (
                    'REFERRAL','HR_EMAIL','CAREERS_SITE','FACEBOOK','TOPCV','ITVIEC',
                    'LINKEDIN','JOBBOARD','HEADHUNTER','WALK_IN','OTHER'
                ) THEN TRUE ELSE "IsSystem" END
                WHERE "IsDeleted" = FALSE;
                """);

            // Grant new source permissions to system Admin + HR (same scope as prior recruitment pack).
            migrationBuilder.Sql("""
                INSERT INTO "RolePermissionEntities"
                    ("Id","RoleId","PermissionCode","DataScope","CreatedBy","CreatedAt","IsDeleted","Version")
                SELECT gen_random_uuid(), r."Id", p.code, 'ALL',
                       '00000000-0000-0000-0000-000000000000'::uuid, NOW(), FALSE, 1
                FROM (
                    SELECT '10000000-0000-0000-0000-000000000001'::uuid AS "Id"
                    UNION ALL
                    SELECT '10000000-0000-0000-0000-000000000002'::uuid
                ) r
                CROSS JOIN (
                    SELECT 'RECRUITMENT_SOURCE_VIEW' AS code
                    UNION ALL
                    SELECT 'RECRUITMENT_SOURCE_MANAGE'
                ) p
                WHERE NOT EXISTS (
                    SELECT 1 FROM "RolePermissionEntities" rp
                    WHERE rp."RoleId" = r."Id"
                      AND rp."PermissionCode" = p.code
                      AND rp."IsDeleted" = FALSE
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HiringSourceEntities_DisplayOrder",
                table: "HiringSourceEntities");

            migrationBuilder.DropColumn(
                name: "ChannelType",
                table: "HiringSourceEntities");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "HiringSourceEntities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "HiringSourceEntities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "HiringSourceEntities");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "HiringSourceEntities");
        }
    }
}
