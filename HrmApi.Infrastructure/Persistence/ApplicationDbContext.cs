using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.AuditLog;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Entities.Timekeeping;
using Microsoft.EntityFrameworkCore;

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
        /* Employee */
        public DbSet<EmployeeEntity> EmployeeEntities { get; set; }
        public DbSet<EmployeeDependentEntity> EmployeeDependentEntities { get; set; }
        public DbSet<EmployeeEducationEntity> EmployeeEducationEntities { get; set; }
        public DbSet<EmployeeCertificateEntity> EmployeeCertificateEntities { get; set; }
        public DbSet<EmployeeFileEntity> EmployeeFileEntities { get; set; }
        public DbSet<EmployeeSalaryHistoryEntity> EmployeeSalaryHistoryEntities { get; set; }
        /* Timekeeping */
        public DbSet<TimeKeepingStandardEntity> TimeKeepingStandardEntities { get; set; }
        public DbSet<ShiftMasterEntity> ShiftMasterEntities { get; set; }
        public DbSet<ShiftEntity> ShiftEntities { get; set; }
        public DbSet<WorkScheduledEmployeeEntity> WorkScheduledEmployeeEntities { get; set; }
        public DbSet<TimekeepingEntity> TimekeepingEntities { get; set; }
        public DbSet<TimekeepingSummaryEntity> TimekeepingSummaryEntities { get; set; }
        /* Leave */
        public DbSet<DayOffConfigEntity> DayOffConfigEntities { get; set; }
        public DbSet<DayOffConfigEmployeeEntity> DayOffConfigEmployeeEntities { get; set; }
        public DbSet<PublicHolidayEntity> PublicHolidayEntities { get; set; }
        public DbSet<RegisterDayOffEntity> RegisterDayOffEntities { get; set; }
        /* User - Permission - Role */
        public DbSet<UserEntity> UserEntities { get; set; }
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

            modelBuilder.Entity<TimekeepingEntity>()
                .HasIndex(x => new { x.EmployeeId, x.WorkDate })
                .IsUnique();

            modelBuilder.Entity<TimekeepingSummaryEntity>()
                .HasIndex(x => new { x.EmployeeId, x.Year, x.Month })
                .IsUnique();

            modelBuilder.Entity<WorkScheduledEmployeeEntity>()
                .HasIndex(x => new { x.EmployeeId, x.WorkDate });
        }
    }
}
