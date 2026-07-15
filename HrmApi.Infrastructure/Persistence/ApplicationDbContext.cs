using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Entities.AuditLog;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        /* Organizition */
        public DbSet<CompanyEntity> CompanyEntities { get; set; }
        public DbSet<BranchEntity> BranchEntities { get; set; }
        public DbSet<DepartmentEntity> DepartmentEntities { get; set; }
        public DbSet<PartEntity> PartEntities { get; set; }
        public DbSet<PartMasterEntity> PartMasterEntities { get; set; }
        public DbSet<PositionEntity> PositionEntities { get; set; }
        public DbSet<PositionMasterEntity> PositionMasterEntities { get; set; }
        /* User - Permission - Role */
        public DbSet<UserEntity> UserEntities{ get; set; }
        public DbSet<RoleEntity> RoleEntities { get; set; }
        public DbSet<PermissionEntity> PermissionEntities { get; set; }
        public DbSet<RolePermissionEntity> RolePermissionEntities { get; set; }
        public DbSet<UserRoleEntity> UserRoleEntities { get; set; }
        public DbSet<UserTokenEntity> UserTokenEntities { get; set; }
        public DbSet<ActionLogEntity> ActionLogEntities { get; set; }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
