using HrmApi.Domain.Entities.Asset;
using HrmApi.Domain.Entities.AuditLog;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Entities.Discipline;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.EmployeeMovement;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Entities.Notification;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Entities.Performance;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Entities.Recruitment;
using HrmApi.Domain.Entities.Settings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Entities.Training;
using HrmApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<NotificationEntity> NotificationEntities { get; }
        DbSet<DeviceTokenEntity> DeviceTokenEntities { get; }
        DbSet<NotificationSettingEntity> NotificationSettingEntities { get; }
        DbSet<CompanyEntity> CompanyEntities { get; }
        DbSet<CompanyAnnouncementEntity> CompanyAnnouncementEntities { get; }
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
        DbSet<OvertimeRequestEntity> OvertimeRequestEntities { get; }
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

        DbSet<JobDescriptionEntity> JobDescriptionEntities { get; }
        DbSet<EvaluationCriteriaEntity> EvaluationCriteriaEntities { get; }
        DbSet<HiringSourceEntity> HiringSourceEntities { get; }
        DbSet<RecruitmentRequestEntity> RecruitmentRequestEntities { get; }
        DbSet<HiringPlanEntity> HiringPlanEntities { get; }
        DbSet<HiringPlanCriteriaEntity> HiringPlanCriteriaEntities { get; }
        DbSet<CandidateEntity> CandidateEntities { get; }
        DbSet<InterviewScheduleEntity> InterviewScheduleEntities { get; }
        DbSet<InterviewInterviewerEntity> InterviewInterviewerEntities { get; }
        DbSet<InterviewEvaluationEntity> InterviewEvaluationEntities { get; }

        DbSet<ViolationTypeEntity> ViolationTypeEntities { get; }
        DbSet<ViolationEntity> ViolationEntities { get; }
        DbSet<PerformanceReviewCycleEntity> PerformanceReviewCycleEntities { get; }
        DbSet<KpiGoalEntity> KpiGoalEntities { get; }
        DbSet<KpiResultEntity> KpiResultEntities { get; }
        DbSet<CompetencyFrameworkEntity> CompetencyFrameworkEntities { get; }
        DbSet<Performance360ReviewEntity> Performance360ReviewEntities { get; }
        DbSet<TrainingCourseEntity> TrainingCourseEntities { get; }
        DbSet<TrainingCourseMaterialEntity> TrainingCourseMaterialEntities { get; }
        DbSet<TrainingQuizEntity> TrainingQuizEntities { get; }
        DbSet<TrainingEnrollmentEntity> TrainingEnrollmentEntities { get; }
        DbSet<TrainingResultEntity> TrainingResultEntities { get; }

        DbSet<AssetTypeEntity> AssetTypeEntities { get; }
        DbSet<AssetEntity> AssetEntities { get; }
        DbSet<AssetTicketEntity> AssetTicketEntities { get; }
        DbSet<AssetAssignmentEntity> AssetAssignmentEntities { get; }

        DbSet<ReportScheduleEntity> ReportScheduleEntities { get; }
        DbSet<LegalRateConfigEntity> LegalRateConfigEntities { get; }
        DbSet<NotificationTemplateEntity> NotificationTemplateEntities { get; }
        DbSet<ApiClientKeyEntity> ApiClientKeyEntities { get; }
        DbSet<WebhookSubscriptionEntity> WebhookSubscriptionEntities { get; }
        DbSet<SystemRetentionConfigEntity> SystemRetentionConfigEntities { get; }
        DbSet<SmsGatewayConfigEntity> SmsGatewayConfigEntities { get; }
        DbSet<ZaloOaConfigEntity> ZaloOaConfigEntities { get; }
        DbSet<IpAllowlistEntryEntity> IpAllowlistEntryEntities { get; }

        DbSet<WorkflowDefinitionEntity> WorkflowDefinitionEntities { get; }
        DbSet<WorkflowStepEntity> WorkflowStepEntities { get; }
        DbSet<WorkflowInstanceEntity> WorkflowInstanceEntities { get; }
        DbSet<WorkflowTaskEntity> WorkflowTaskEntities { get; }
        DbSet<WorkflowFormTemplateEntity> WorkflowFormTemplateEntities { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
