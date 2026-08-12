using HrmApi.Domain.Entities.AuditLog;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.EmployeeMovement;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Entities.Timekeeping;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<CompanyEntity> CompanyEntities { get; }
        DbSet<BranchEntity> BranchEntities { get; }
        DbSet<DepartmentEntity> DepartmentEntities { get; }
        DbSet<PositionEntity> PositionEntities { get; }
        DbSet<PositionMasterEntity> PositionMasterEntities { get; }
        DbSet<PartEntity> PartEntities { get; }
        DbSet<PartMasterEntity> PartMasterEntities { get; }
        DbSet<EmployeeEntity> EmployeeEntities { get; }
        DbSet<EmployeeDependentEntity> EmployeeDependentEntities { get; }
        DbSet<EmployeeEducationEntity> EmployeeEducationEntities { get; }
        DbSet<EmployeeCertificateEntity> EmployeeCertificateEntities { get; }
        DbSet<EmployeeFileEntity> EmployeeFileEntities { get; }
        DbSet<EmployeeSalaryHistoryEntity> EmployeeSalaryHistoryEntities { get; }
        DbSet<TimeKeepingStandardEntity> TimeKeepingStandardEntities { get; }
        DbSet<ShiftMasterEntity> ShiftMasterEntities { get; }
        DbSet<ShiftEntity> ShiftEntities { get; }
        DbSet<WorkScheduledEmployeeEntity> WorkScheduledEmployeeEntities { get; }
        DbSet<EmployeeWorkPatternEntity> EmployeeWorkPatternEntities { get; }
        DbSet<TimekeepingEntity> TimekeepingEntities { get; }
        DbSet<TimekeepingSummaryEntity> TimekeepingSummaryEntities { get; }
        DbSet<AttendanceComplaintEntity> AttendanceComplaintEntities { get; }
        DbSet<DayOffConfigEntity> DayOffConfigEntities { get; }
        DbSet<DayOffConfigEmployeeEntity> DayOffConfigEmployeeEntities { get; }
        DbSet<PublicHolidayEntity> PublicHolidayEntities { get; }
        DbSet<RegisterDayOffEntity> RegisterDayOffEntities { get; }
        DbSet<UserEntity> UserEntities { get; }
        DbSet<RoleEntity> RoleEntities { get; }
        DbSet<UserRoleEntity> UserRoleEntities { get; }
        DbSet<UserTokenEntity> UserTokenEntities { get; }
        DbSet<RolePermissionEntity> RolePermissionEntities { get; }
        DbSet<ActionLogEntity> ActionLogEntities { get; }
        DbSet<ContractTypeEntity> ContractTypeEntities { get; }
        DbSet<ContractEntity> ContractEntities { get; }
        DbSet<ReviewRenewalEntity> ReviewRenewalEntities { get; }
        DbSet<TransferEmployeeEntity> TransferEmployeeEntities { get; }
        DbSet<TransferEmployeePositionEntity> TransferEmployeePositionEntities { get; }

        DbSet<SalaryConfigEntity> SalaryConfigEntities { get; }
        DbSet<SalaryEntity> SalaryEntities { get; }
        DbSet<SalaryLineItemEntity> SalaryLineItemEntities { get; }
        DbSet<AllowanceEntity> AllowanceEntities { get; }
        DbSet<AdvanceEntity> AdvanceEntities { get; }
        DbSet<DeductionSlipEntity> DeductionSlipEntities { get; }
        DbSet<CashAdditionSlipEntity> CashAdditionSlipEntities { get; }
        DbSet<SalaryCoefficientEntity> SalaryCoefficientEntities { get; }
        DbSet<SalaryIncreaseEntity> SalaryIncreaseEntities { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
