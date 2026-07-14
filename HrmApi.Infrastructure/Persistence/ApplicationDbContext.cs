using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities;
using HrmApi.Domain.Entities.Organization;
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
        public DbSet<CompanyEntity> CompanyEntities { get; set; }
        public DbSet<BranchEntity> BranchEntities { get; set; }
        public DbSet<DepartmentEntity> DepartmentEntities { get; set; }
        public DbSet<PartEntity> PartEntities { get; set; }
        public DbSet<PartMasterEntity> PartMasterEntities { get; set; }
        public DbSet<PositionEntity> PositionEntities { get; set; }
        public DbSet<PositionMasterEntity> PositionMasterEntities { get; set; }

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
