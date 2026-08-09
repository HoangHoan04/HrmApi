using System.Threading;
using System.Threading.Tasks;
using HrmApi.Domain.Entities.AuditLog;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Entities.Timekeeping;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        /* Organizition */
        DbSet<CompanyEntity> CompanyEntities { get; }
        DbSet<BranchEntity> BranchEntities { get; }
        DbSet<DepartmentEntity> DepartmentEntities { get; }
        DbSet<PositionEntity> PositionEntities { get; }
        DbSet<PositionMasterEntity> PositionMasterEntities { get; }
        DbSet<PartEntity> PartEntities { get; }
        DbSet<PartMasterEntity> PartMasterEntities { get; }
        /* Employee */
        DbSet<EmployeeEntity> EmployeeEntities { get; }
        DbSet<EmployeeDependentEntity> EmployeeDependentEntities { get; }
        DbSet<EmployeeEducationEntity> EmployeeEducationEntities { get; }
        DbSet<EmployeeCertificateEntity> EmployeeCertificateEntities { get; }
        DbSet<EmployeeFileEntity> EmployeeFileEntities { get; }
        DbSet<EmployeeSalaryHistoryEntity> EmployeeSalaryHistoryEntities { get; }
        /* Timekeeping */
        DbSet<TimeKeepingStandardEntity> TimeKeepingStandardEntities { get; }
        DbSet<ShiftMasterEntity> ShiftMasterEntities { get; }
        DbSet<ShiftEntity> ShiftEntities { get; }
        DbSet<WorkScheduledEmployeeEntity> WorkScheduledEmployeeEntities { get; }
        DbSet<TimekeepingEntity> TimekeepingEntities { get; }
        DbSet<TimekeepingSummaryEntity> TimekeepingSummaryEntities { get; }
        /* Leave */
        DbSet<DayOffConfigEntity> DayOffConfigEntities { get; }
        DbSet<DayOffConfigEmployeeEntity> DayOffConfigEmployeeEntities { get; }
        DbSet<PublicHolidayEntity> PublicHolidayEntities { get; }
        DbSet<RegisterDayOffEntity> RegisterDayOffEntities { get; }
        /* User - Permission - Role */
        DbSet<UserEntity> UserEntities { get; }
        DbSet<RoleEntity> RoleEntities { get; }
        DbSet<UserRoleEntity> UserRoleEntities { get; }
        DbSet<UserTokenEntity> UserTokenEntities { get; }
        DbSet<PermissionEntity> PermissionEntities { get; }
        DbSet<RolePermissionEntity> RolePermissionEntities { get; }
        DbSet<ActionLogEntity> ActionLogEntities { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
