using HrmApi.Application.Common.Interfaces;
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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HrmApi.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<NotificationEntity> NotificationEntities { get; set; }
        public DbSet<DeviceTokenEntity> DeviceTokenEntities { get; set; }
        public DbSet<NotificationSettingEntity> NotificationSettingEntities { get; set; }

        public DbSet<CompanyEntity> CompanyEntities { get; set; }
        public DbSet<CompanyAnnouncementEntity> CompanyAnnouncementEntities { get; set; }
        public DbSet<BranchEntity> BranchEntities { get; set; }
        public DbSet<DepartmentEntity> DepartmentEntities { get; set; }
        public DbSet<PartEntity> PartEntities { get; set; }
        public DbSet<PartMasterEntity> PartMasterEntities { get; set; }
        public DbSet<PositionEntity> PositionEntities { get; set; }
        public DbSet<PositionMasterEntity> PositionMasterEntities { get; set; }

        public DbSet<EmployeeEntity> EmployeeEntities { get; set; }
        public DbSet<EmployeeDependentEntity> EmployeeDependentEntities { get; set; }
        public DbSet<EmployeeEducationEntity> EmployeeEducationEntities { get; set; }
        public DbSet<EmployeeCertificateEntity> EmployeeCertificateEntities { get; set; }
        public DbSet<EmployeeFileEntity> EmployeeFileEntities { get; set; }
        public DbSet<EmployeeSalaryHistoryEntity> EmployeeSalaryHistoryEntities { get; set; }

        public DbSet<TransferEmployeeEntity> TransferEmployeeEntities { get; set; }
        public DbSet<TransferEmployeePositionEntity> TransferEmployeePositionEntities { get; set; }

        public DbSet<ContractTypeEntity> ContractTypeEntities { get; set; }
        public DbSet<ContractEntity> ContractEntities { get; set; }
        public DbSet<ReviewRenewalEntity> ReviewRenewalEntities { get; set; }

        public DbSet<SalaryConfigEntity> SalaryConfigEntities { get; set; }
        public DbSet<SalaryEntity> SalaryEntities { get; set; }
        public DbSet<SalaryLineItemEntity> SalaryLineItemEntities { get; set; }
        public DbSet<AllowanceEntity> AllowanceEntities { get; set; }
        public DbSet<AdvanceEntity> AdvanceEntities { get; set; }
        public DbSet<DeductionSlipEntity> DeductionSlipEntities { get; set; }
        public DbSet<CashAdditionSlipEntity> CashAdditionSlipEntities { get; set; }
        public DbSet<SalaryCoefficientEntity> SalaryCoefficientEntities { get; set; }
        public DbSet<SalaryIncreaseEntity> SalaryIncreaseEntities { get; set; }

        public DbSet<TimeKeepingStandardEntity> TimeKeepingStandardEntities { get; set; }
        public DbSet<ShiftMasterEntity> ShiftMasterEntities { get; set; }
        public DbSet<ShiftEntity> ShiftEntities { get; set; }
        public DbSet<WorkScheduledEmployeeEntity> WorkScheduledEmployeeEntities { get; set; }
        public DbSet<EmployeeWorkPatternEntity> EmployeeWorkPatternEntities { get; set; }
        public DbSet<TimekeepingEntity> TimekeepingEntities { get; set; }
        public DbSet<TimekeepingSummaryEntity> TimekeepingSummaryEntities { get; set; }
        public DbSet<AttendanceComplaintEntity> AttendanceComplaintEntities { get; set; }
        public DbSet<OvertimeRequestEntity> OvertimeRequestEntities { get; set; }

        public DbSet<DayOffConfigEntity> DayOffConfigEntities { get; set; }
        public DbSet<DayOffConfigEmployeeEntity> DayOffConfigEmployeeEntities { get; set; }
        public DbSet<PublicHolidayEntity> PublicHolidayEntities { get; set; }
        public DbSet<RegisterDayOffEntity> RegisterDayOffEntities { get; set; }

        public DbSet<UserEntity> UserEntities { get; set; }
        public DbSet<RoleEntity> RoleEntities { get; set; }
        public DbSet<RolePermissionEntity> RolePermissionEntities { get; set; }
        public DbSet<UserRoleEntity> UserRoleEntities { get; set; }
        public DbSet<UserTokenEntity> UserTokenEntities { get; set; }
        public DbSet<ActionLogEntity> ActionLogEntities { get; set; }

        public DbSet<JobDescriptionEntity> JobDescriptionEntities { get; set; }
        public DbSet<EvaluationCriteriaEntity> EvaluationCriteriaEntities { get; set; }
        public DbSet<HiringSourceEntity> HiringSourceEntities { get; set; }
        public DbSet<RecruitmentRequestEntity> RecruitmentRequestEntities { get; set; }
        public DbSet<HiringPlanEntity> HiringPlanEntities { get; set; }
        public DbSet<HiringPlanCriteriaEntity> HiringPlanCriteriaEntities { get; set; }
        public DbSet<CandidateEntity> CandidateEntities { get; set; }
        public DbSet<InterviewScheduleEntity> InterviewScheduleEntities { get; set; }
        public DbSet<InterviewInterviewerEntity> InterviewInterviewerEntities { get; set; }
        public DbSet<InterviewEvaluationEntity> InterviewEvaluationEntities { get; set; }

        public DbSet<ViolationTypeEntity> ViolationTypeEntities { get; set; }
        public DbSet<ViolationEntity> ViolationEntities { get; set; }
        public DbSet<PerformanceReviewCycleEntity> PerformanceReviewCycleEntities { get; set; }
        public DbSet<KpiGoalEntity> KpiGoalEntities { get; set; }
        public DbSet<KpiResultEntity> KpiResultEntities { get; set; }
        public DbSet<CompetencyFrameworkEntity> CompetencyFrameworkEntities { get; set; }
        public DbSet<Performance360ReviewEntity> Performance360ReviewEntities { get; set; }
        public DbSet<TrainingCourseEntity> TrainingCourseEntities { get; set; }
        public DbSet<TrainingCourseMaterialEntity> TrainingCourseMaterialEntities { get; set; }
        public DbSet<TrainingQuizEntity> TrainingQuizEntities { get; set; }
        public DbSet<TrainingEnrollmentEntity> TrainingEnrollmentEntities { get; set; }
        public DbSet<TrainingResultEntity> TrainingResultEntities { get; set; }

        public DbSet<AssetTypeEntity> AssetTypeEntities { get; set; }
        public DbSet<AssetEntity> AssetEntities { get; set; }
        public DbSet<AssetTicketEntity> AssetTicketEntities { get; set; }

        public DbSet<ReportScheduleEntity> ReportScheduleEntities { get; set; }
        public DbSet<LegalRateConfigEntity> LegalRateConfigEntities { get; set; }
        public DbSet<NotificationTemplateEntity> NotificationTemplateEntities { get; set; }
        public DbSet<ApiClientKeyEntity> ApiClientKeyEntities { get; set; }
        public DbSet<WebhookSubscriptionEntity> WebhookSubscriptionEntities { get; set; }
        public DbSet<SystemRetentionConfigEntity> SystemRetentionConfigEntities { get; set; }
        public DbSet<SmsGatewayConfigEntity> SmsGatewayConfigEntities { get; set; }
        public DbSet<ZaloOaConfigEntity> ZaloOaConfigEntities { get; set; }
        public DbSet<IpAllowlistEntryEntity> IpAllowlistEntryEntities { get; set; }

        public DbSet<WorkflowDefinitionEntity> WorkflowDefinitionEntities { get; set; }
        public DbSet<WorkflowStepEntity> WorkflowStepEntities { get; set; }
        public DbSet<WorkflowInstanceEntity> WorkflowInstanceEntities { get; set; }
        public DbSet<WorkflowTaskEntity> WorkflowTaskEntities { get; set; }
        public DbSet<WorkflowFormTemplateEntity> WorkflowFormTemplateEntities { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EnsureUtcDateTimes();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            EnsureUtcDateTimes();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        private void EnsureUtcDateTimes()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    foreach (var property in entry.Properties)
                    {
                        if (property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                        }
                    }
                }
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<DateTime>()
                .HaveConversion<UtcDateTimeConverter>();

            configurationBuilder.Properties<DateTime?>()
                .HaveConversion<NullableUtcDateTimeConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureOrganization(modelBuilder);
            ConfigureEmployee(modelBuilder);
            ConfigureEmployeeMovement(modelBuilder);
            ConfigureContract(modelBuilder);
            ConfigurePayroll(modelBuilder);
            ConfigurePermission(modelBuilder);
            ConfigureTimekeeping(modelBuilder);
            ConfigureLeave(modelBuilder);
            ConfigureRecruitment(modelBuilder);
            ConfigureDiscipline(modelBuilder);
            ConfigurePerformance(modelBuilder);
            ConfigureTraining(modelBuilder);
            ConfigureAsset(modelBuilder);
            ConfigureSettings(modelBuilder);
            ConfigureWorkflow(modelBuilder);
            ConfigureAudit(modelBuilder);
        }

        private static void ConfigureOrganization(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<CompanyEntity>(entity =>
            {
                _ = entity.HasOne(c => c.ParentCompany)
                    .WithMany(c => c.ChildCompanies)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(c => c.TimeKeepingStandard)
                    .WithMany()
                    .HasForeignKey(c => c.TimeKeepingStandardId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<CompanyAnnouncementEntity>(entity =>
            {
                _ = entity.Property(x => x.Title).HasMaxLength(300);
                _ = entity.HasIndex(x => new { x.CompanyId, x.PublishedAt });
                _ = entity.HasIndex(x => new { x.CompanyId, x.IsActive });
            });

            _ = modelBuilder.Entity<BranchEntity>(entity =>
            {
                _ = entity.HasOne(b => b.Company)
                    .WithMany(c => c.Branches)
                    .HasForeignKey(b => b.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(b => b.ParentBranch)
                    .WithMany(b => b.ChildBranches)
                    .HasForeignKey(b => b.ParentBranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(b => b.Manager)
                    .WithMany()
                    .HasForeignKey(b => b.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(b => b.TimeKeepingStandard)
                    .WithMany(t => t.BranchEntities)
                    .HasForeignKey(b => b.TimeKeepingStandardId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<DepartmentEntity>(entity =>
            {
                _ = entity.HasOne(d => d.Company)
                    .WithMany(c => c.DepartmentEntities)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.Branch)
                    .WithMany(b => b.Departments)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.ParentDepartment)
                    .WithMany(d => d.ChildDepartments)
                    .HasForeignKey(d => d.ParentDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.Manager)
                    .WithMany()
                    .HasForeignKey(d => d.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.DeputyManager)
                    .WithMany()
                    .HasForeignKey(d => d.DeputyManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<PartMasterEntity>(entity =>
            {
                _ = entity.HasOne(p => p.Company)
                    .WithMany(c => c.PartMasterEntities)
                    .HasForeignKey(p => p.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.Branch)
                    .WithMany(b => b.PartMasters)
                    .HasForeignKey(p => p.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<PartEntity>(entity =>
            {
                _ = entity.HasOne(p => p.Company)
                    .WithMany(c => c.PartEntities)
                    .HasForeignKey(p => p.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.Branch)
                    .WithMany(b => b.PartEntities)
                    .HasForeignKey(p => p.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.PartMaster)
                    .WithMany(m => m.Parts)
                    .HasForeignKey(p => p.PartMasterId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.Department)
                    .WithMany(d => d.PartEntities)
                    .HasForeignKey(p => p.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.Manager)
                    .WithMany()
                    .HasForeignKey(p => p.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<PositionMasterEntity>(entity =>
            {
                _ = entity.HasOne(p => p.Company)
                    .WithMany(c => c.PositionMasterEntities)
                    .HasForeignKey(p => p.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.Branch)
                    .WithMany(b => b.PositionMasterEntities)
                    .HasForeignKey(p => p.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<PositionEntity>(entity =>
            {
                _ = entity.HasOne(p => p.Company)
                    .WithMany(c => c.PositionEntities)
                    .HasForeignKey(p => p.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.Branch)
                    .WithMany(b => b.PositionEntities)
                    .HasForeignKey(p => p.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.PositionMaster)
                    .WithMany(m => m.Positions)
                    .HasForeignKey(p => p.PositionMasterId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.Department)
                    .WithMany(d => d.PositionEntities)
                    .HasForeignKey(p => p.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(p => p.Part)
                    .WithMany(part => part.PositionEntities)
                    .HasForeignKey(p => p.PartId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureEmployee(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<EmployeeEntity>(entity =>
            {
                _ = entity.HasOne(e => e.Company)
                    .WithMany(c => c.EmployeeEntities)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(e => e.Branch)
                    .WithMany(b => b.EmployeeEntities)
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(e => e.Department)
                    .WithMany(d => d.EmployeeEntities)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(e => e.Part)
                    .WithMany(p => p.EmployeeEntities)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(e => e.Position)
                    .WithMany(p => p.EmployeeEntities)
                    .HasForeignKey(e => e.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(e => e.DirectManager)
                    .WithMany(e => e.DirectReports)
                    .HasForeignKey(e => e.DirectManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<EmployeeDependentEntity>(entity =>
            {
                _ = entity.HasOne(d => d.Employee)
                    .WithMany(e => e.Dependents)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<EmployeeEducationEntity>(entity =>
            {
                _ = entity.HasOne(d => d.Employee)
                    .WithMany(e => e.Educations)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<EmployeeCertificateEntity>(entity =>
            {
                _ = entity.HasOne(d => d.Employee)
                    .WithMany(e => e.Certificates)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<EmployeeFileEntity>(entity =>
            {
                _ = entity.HasOne(d => d.Employee)
                    .WithMany(e => e.Files)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasOne(d => d.ReplacesFile)
                    .WithMany()
                    .HasForeignKey(d => d.ReplacesFileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<EmployeeSalaryHistoryEntity>(entity =>
            {
                _ = entity.HasOne(d => d.Employee)
                    .WithMany(e => e.SalaryHistories)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureEmployeeMovement(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<TransferEmployeeEntity>(entity =>
            {
                _ = entity.HasOne(t => t.Employee)
                    .WithMany(e => e.TransferEmployees)
                    .HasForeignKey(t => t.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasIndex(t => new { t.EmployeeId, t.EffectiveDate });
            });

            _ = modelBuilder.Entity<TransferEmployeePositionEntity>(entity =>
            {
                _ = entity.HasOne(d => d.TransferEmployee)
                    .WithMany(t => t.TransferDetails)
                    .HasForeignKey(d => d.TransferEmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasOne(d => d.Employee)
                    .WithMany(e => e.TransferEmployeePositions)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.OldCompany)
                    .WithMany()
                    .HasForeignKey(d => d.OldCompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.NewCompany)
                    .WithMany()
                    .HasForeignKey(d => d.NewCompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.OldBranch)
                    .WithMany()
                    .HasForeignKey(d => d.OldBranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.NewBranch)
                    .WithMany()
                    .HasForeignKey(d => d.NewBranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.OldDepartment)
                    .WithMany()
                    .HasForeignKey(d => d.OldDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.NewDepartment)
                    .WithMany()
                    .HasForeignKey(d => d.NewDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.OldPart)
                    .WithMany()
                    .HasForeignKey(d => d.OldPartId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.NewPart)
                    .WithMany()
                    .HasForeignKey(d => d.NewPartId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.OldPosition)
                    .WithMany()
                    .HasForeignKey(d => d.OldPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.NewPosition)
                    .WithMany()
                    .HasForeignKey(d => d.NewPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(d => d.EmployeeId);
            });
        }

        private static void ConfigureContract(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<ContractTypeEntity>(entity =>
            {
                _ = entity.HasOne(t => t.Company)
                    .WithMany()
                    .HasForeignKey(t => t.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(t => t.Code).IsUnique();
            });

            _ = modelBuilder.Entity<ContractEntity>(entity =>
            {
                _ = entity.HasOne(c => c.Employee)
                    .WithMany(e => e.Contracts)
                    .HasForeignKey(c => c.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasOne(c => c.ContractType)
                    .WithMany(t => t.Contracts)
                    .HasForeignKey(c => c.ContractTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(c => c.Company)
                    .WithMany()
                    .HasForeignKey(c => c.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(c => c.Branch)
                    .WithMany()
                    .HasForeignKey(c => c.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(c => c.Department)
                    .WithMany()
                    .HasForeignKey(c => c.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(c => c.Part)
                    .WithMany()
                    .HasForeignKey(c => c.PartId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(c => c.Position)
                    .WithMany()
                    .HasForeignKey(c => c.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(c => c.PreviousContract)
                    .WithMany(c => c.RenewedContracts)
                    .HasForeignKey(c => c.PreviousContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(c => c.Code).IsUnique();
                _ = entity.HasIndex(c => new { c.EmployeeId, c.Status });
                _ = entity.HasIndex(c => c.EndDate);
            });

            _ = modelBuilder.Entity<ReviewRenewalEntity>(entity =>
            {
                _ = entity.HasOne(r => r.Contract)
                    .WithMany(c => c.ReviewRenewals)
                    .HasForeignKey(r => r.ContractId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasOne(r => r.Employee)
                    .WithMany()
                    .HasForeignKey(r => r.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(r => r.ProposedContractType)
                    .WithMany()
                    .HasForeignKey(r => r.ProposedContractTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(r => r.NewContract)
                    .WithMany()
                    .HasForeignKey(r => r.NewContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(r => new { r.ContractId, r.Status });
            });
        }

        private static void ConfigurePermission(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<UserEntity>(entity =>
            {
                _ = entity.HasOne(u => u.Company)
                    .WithMany(c => c.UserEntities)
                    .HasForeignKey(u => u.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(u => u.Branch)
                    .WithMany(b => b.UserEntities)
                    .HasForeignKey(u => u.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(u => u.Employee)
                    .WithOne(e => e.User)
                    .HasForeignKey<UserEntity>(u => u.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Wave B5: login lookup
                _ = entity.HasIndex(u => u.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_UserEntities_Username");
            });

            _ = modelBuilder.Entity<RoleEntity>(entity =>
            {
                _ = entity.HasOne(r => r.Company)
                    .WithMany(c => c.RoleEntities)
                    .HasForeignKey(r => r.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(r => r.Branch)
                    .WithMany(b => b.RoleEntities)
                    .HasForeignKey(r => r.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<UserRoleEntity>(entity =>
            {
                _ = entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<RolePermissionEntity>(entity =>
            {
                _ = entity.Property(rp => rp.PermissionCode)
                    .HasMaxLength(100)
                    .IsRequired();

                _ = entity.HasIndex(rp => new { rp.RoleId, rp.PermissionCode })
                    .IsUnique()
                    .HasFilter("\"IsDeleted\" = FALSE");

                _ = entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<UserTokenEntity>(entity =>
            {
                _ = entity.HasOne(t => t.User)
                    .WithMany(u => u.UserTokens)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasOne<UserTokenEntity>()
                    .WithMany()
                    .HasForeignKey(t => t.ReplacedByTokenId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Wave B5: lookup refresh token nhanh
                _ = entity.HasIndex(t => t.RefreshTokenHash)
                    .HasDatabaseName("IX_UserTokenEntities_RefreshTokenHash");
            });
        }

        private static void ConfigureTimekeeping(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<TimeKeepingStandardEntity>(entity =>
            {
                _ = entity.HasOne(t => t.Company)
                    .WithMany(c => c.TimeKeepingStandardEntities)
                    .HasForeignKey(t => t.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<ShiftMasterEntity>(entity =>
            {
                _ = entity.HasOne(s => s.Company)
                    .WithMany(c => c.ShiftMasterEntities)
                    .HasForeignKey(s => s.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<ShiftEntity>(entity =>
            {
                _ = entity.HasOne(s => s.ShiftMaster)
                    .WithMany(m => m.ShiftEntities)
                    .HasForeignKey(s => s.ShiftMasterId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(s => s.Branch)
                    .WithMany(b => b.ShiftEntities)
                    .HasForeignKey(s => s.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<WorkScheduledEmployeeEntity>(entity =>
            {
                _ = entity.HasOne(w => w.Employee)
                    .WithMany()
                    .HasForeignKey(w => w.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(w => w.Shift)
                    .WithMany(s => s.WorkScheduledEmployeeEntities)
                    .HasForeignKey(w => w.ShiftId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(w => w.ShiftMaster)
                    .WithMany(s => s.WorkScheduledEmployeeEntities)
                    .HasForeignKey(w => w.ShiftMasterId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(w => w.Branch)
                    .WithMany(b => b.WorkScheduledEmployeeEntities)
                    .HasForeignKey(w => w.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(x => new { x.EmployeeId, x.WorkDate });
            });

            _ = modelBuilder.Entity<EmployeeWorkPatternEntity>(entity =>
            {
                _ = entity.Property(e => e.PatternType).HasConversion<string>();

                _ = entity.HasOne(w => w.Employee)
                    .WithMany()
                    .HasForeignKey(w => w.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(w => w.ShiftMaster)
                    .WithMany()
                    .HasForeignKey(w => w.ShiftMasterId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(w => w.Branch)
                    .WithMany()
                    .HasForeignKey(w => w.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom, x.IsActive });
            });

            _ = modelBuilder.Entity<TimekeepingEntity>(entity =>
            {
                _ = entity.Property(e => e.Status).HasConversion<string>();

                _ = entity.HasOne(t => t.Employee)
                    .WithMany()
                    .HasForeignKey(t => t.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(t => t.Company)
                    .WithMany(c => c.TimekeepingEntities)
                    .HasForeignKey(t => t.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(t => t.Branch)
                    .WithMany(b => b.TimekeepingEntities)
                    .HasForeignKey(t => t.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(t => t.Shift)
                    .WithMany(s => s.TimekeepingEntities)
                    .HasForeignKey(t => t.ShiftId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(t => t.ShiftMaster)
                    .WithMany(s => s.TimekeepingEntities)
                    .HasForeignKey(t => t.ShiftMasterId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();
            });

            _ = modelBuilder.Entity<TimekeepingSummaryEntity>(entity =>
            {
                _ = entity.HasOne(t => t.Employee)
                    .WithMany()
                    .HasForeignKey(t => t.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(t => t.Company)
                    .WithMany(c => c.TimekeepingSummaryEntities)
                    .HasForeignKey(t => t.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(t => t.Branch)
                    .WithMany(b => b.TimekeepingSummaryEntities)
                    .HasForeignKey(t => t.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(x => new { x.EmployeeId, x.Year, x.Month }).IsUnique();
            });

            _ = modelBuilder.Entity<AttendanceComplaintEntity>(entity =>
            {
                _ = entity.Property(e => e.Status).HasConversion<string>();
                _ = entity.Property(e => e.ComplaintType).HasConversion<string>();
                _ = entity.Property(e => e.Reason).HasMaxLength(1000);

                _ = entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(e => e.Timekeeping)
                    .WithMany()
                    .HasForeignKey(e => e.TimekeepingId)
                    .OnDelete(DeleteBehavior.SetNull);

                _ = entity.HasOne(e => e.Approver)
                    .WithMany()
                    .HasForeignKey(e => e.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(x => new { x.EmployeeId, x.WorkDate, x.Status });
            });

            _ = modelBuilder.Entity<OvertimeRequestEntity>(entity =>
            {
                _ = entity.Property(e => e.Code).HasMaxLength(50);
                _ = entity.Property(e => e.OtType).HasMaxLength(30);
                _ = entity.Property(e => e.Status).HasMaxLength(30);
                _ = entity.Property(e => e.Reason).HasMaxLength(1000);
                _ = entity.Property(e => e.ApproverNote).HasMaxLength(1000);
                _ = entity.Property(e => e.AttachmentUrl).HasMaxLength(500);

                _ = entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(e => e.Approver)
                    .WithMany()
                    .HasForeignKey(e => e.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(x => x.Code).IsUnique();
                _ = entity.HasIndex(x => new { x.EmployeeId, x.WorkDate, x.Status });
            });
        }

        private static void ConfigureLeave(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<DayOffConfigEntity>(entity =>
            {
                _ = entity.HasOne(d => d.Company)
                    .WithMany(c => c.DayOffConfigEntities)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<DayOffConfigEmployeeEntity>(entity =>
            {
                _ = entity.HasOne(d => d.DayOffConfig)
                    .WithMany(c => c.DayOffConfigEmployeeEntities)
                    .HasForeignKey(d => d.DayOffConfigId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasOne(d => d.Employee)
                    .WithMany()
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<PublicHolidayEntity>(entity =>
            {
                _ = entity.HasOne(p => p.Company)
                    .WithMany(c => c.PublicHolidayEntities)
                    .HasForeignKey(p => p.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<RegisterDayOffEntity>(entity =>
            {
                _ = entity.Property(e => e.Status).HasConversion<string>();

                _ = entity.HasOne(r => r.Employee)
                    .WithMany()
                    .HasForeignKey(r => r.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(r => r.Company)
                    .WithMany(c => c.RegisterDayOffEntities)
                    .HasForeignKey(r => r.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(r => r.Branch)
                    .WithMany(b => b.RegisterDayOffEntities)
                    .HasForeignKey(r => r.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(r => r.DayOffConfig)
                    .WithMany(c => c.RegisterDayOffEntities)
                    .HasForeignKey(r => r.DayOffConfigId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(r => r.Approver)
                    .WithMany()
                    .HasForeignKey(r => r.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigurePayroll(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<SalaryConfigEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasIndex(x => x.Code);
            });

            _ = modelBuilder.Entity<SalaryEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.SalaryConfig)
                    .WithMany(c => c.Salaries)
                    .HasForeignKey(x => x.SalaryConfigId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Branch)
                    .WithMany()
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasIndex(x => new { x.EmployeeId, x.Year, x.Month }).IsUnique();
                _ = entity.HasIndex(x => x.PeriodCode);
                _ = entity.HasIndex(x => x.Status);
            });

            _ = modelBuilder.Entity<SalaryLineItemEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Salary)
                    .WithMany(s => s.LineItems)
                    .HasForeignKey(x => x.SalaryId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasIndex(x => new { x.SalaryId, x.ItemCode });
            });

            _ = modelBuilder.Entity<AllowanceEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasIndex(x => x.Code);
            });

            _ = modelBuilder.Entity<AdvanceEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasIndex(x => new { x.EmployeeId, x.Status });
            });

            _ = modelBuilder.Entity<DeductionSlipEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<CashAdditionSlipEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<SalaryCoefficientEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasIndex(x => x.Code);
            });

            _ = modelBuilder.Entity<SalaryIncreaseEntity>(entity =>
            {
                _ = entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureRecruitment(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<JobDescriptionEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.PositionMaster).WithMany().HasForeignKey(x => x.PositionMasterId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<EvaluationCriteriaEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<HiringSourceEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code).IsUnique();
                _ = entity.HasIndex(x => x.DisplayOrder);
                _ = entity.Property(x => x.ChannelType).HasMaxLength(50);
                _ = entity.Property(x => x.ContactEmail).HasMaxLength(255);
                _ = entity.Property(x => x.Description).HasMaxLength(1000);
            });

            _ = modelBuilder.Entity<RecruitmentRequestEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasIndex(x => new { x.Status, x.CompanyId });
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.JobDescription).WithMany(j => j.RecruitmentRequests).HasForeignKey(x => x.JobDescriptionId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.RequestedByEmployee).WithMany().HasForeignKey(x => x.RequestedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.ApprovedByEmployee).WithMany().HasForeignKey(x => x.ApprovedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<HiringPlanEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasIndex(x => new { x.Status, x.CompanyId });
                _ = entity.HasOne(x => x.RecruitmentRequest).WithMany(r => r.HiringPlans).HasForeignKey(x => x.RecruitmentRequestId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.JobDescription).WithMany(j => j.HiringPlans).HasForeignKey(x => x.JobDescriptionId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<HiringPlanCriteriaEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.HiringPlanId, x.EvaluationCriteriaId }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
                _ = entity.HasOne(x => x.HiringPlan).WithMany(p => p.Criteria).HasForeignKey(x => x.HiringPlanId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.EvaluationCriteria).WithMany(c => c.PlanCriteria).HasForeignKey(x => x.EvaluationCriteriaId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<CandidateEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasIndex(x => new { x.Status, x.HiringPlanId });
                _ = entity.HasOne(x => x.HiringPlan).WithMany(p => p.Candidates).HasForeignKey(x => x.HiringPlanId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.RecruitmentRequest).WithMany(r => r.Candidates).HasForeignKey(x => x.RecruitmentRequestId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.HiringSource).WithMany(s => s.Candidates).HasForeignKey(x => x.HiringSourceId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<InterviewScheduleEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.StartAt, x.EndAt });
                _ = entity.HasIndex(x => new { x.CandidateId, x.Round });
                _ = entity.HasOne(x => x.Candidate).WithMany(c => c.Interviews).HasForeignKey(x => x.CandidateId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.HiringPlan).WithMany(p => p.Interviews).HasForeignKey(x => x.HiringPlanId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<InterviewInterviewerEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.InterviewScheduleId, x.EmployeeId }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
                _ = entity.HasOne(x => x.InterviewSchedule).WithMany(i => i.Interviewers).HasForeignKey(x => x.InterviewScheduleId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<InterviewEvaluationEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.InterviewScheduleId, x.InterviewerEmployeeId, x.EvaluationCriteriaId })
                    .IsUnique()
                    .HasFilter("\"IsDeleted\" = FALSE");
                _ = entity.HasOne(x => x.InterviewSchedule).WithMany(i => i.Evaluations).HasForeignKey(x => x.InterviewScheduleId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.InterviewerEmployee).WithMany().HasForeignKey(x => x.InterviewerEmployeeId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.EvaluationCriteria).WithMany(c => c.Evaluations).HasForeignKey(x => x.EvaluationCriteriaId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureDiscipline(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<ViolationTypeEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code).IsUnique();
                _ = entity.Property(x => x.Severity).HasMaxLength(30);
            });

            _ = modelBuilder.Entity<ViolationEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasIndex(x => new { x.Status, x.CompanyId });
                _ = entity.HasOne(x => x.ViolationType).WithMany(t => t.Violations).HasForeignKey(x => x.ViolationTypeId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigurePerformance(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<PerformanceReviewCycleEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasIndex(x => new { x.Status, x.CompanyId });
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<KpiGoalEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.CycleId, x.EmployeeId });
                _ = entity.HasOne(x => x.Cycle).WithMany(c => c.Goals).HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<KpiResultEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.GoalId);
                _ = entity.HasOne(x => x.Goal).WithMany(g => g.Results).HasForeignKey(x => x.GoalId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.RatedByEmployee).WithMany().HasForeignKey(x => x.RatedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<CompetencyFrameworkEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<Performance360ReviewEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.CycleId);
                _ = entity.HasIndex(x => x.SubjectEmployeeId);
                _ = entity.HasOne(x => x.Cycle).WithMany().HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.SubjectEmployee).WithMany().HasForeignKey(x => x.SubjectEmployeeId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.ReviewerEmployee).WithMany().HasForeignKey(x => x.ReviewerEmployeeId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureTraining(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<TrainingCourseEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code);
                _ = entity.HasIndex(x => new { x.Status, x.CompanyId });
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<TrainingCourseMaterialEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.CourseId, x.DisplayOrder });
                _ = entity.HasOne(x => x.Course).WithMany(c => c.Materials).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<TrainingQuizEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.CourseId);
                _ = entity.HasOne(x => x.Course).WithMany(c => c.Quizzes).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<TrainingEnrollmentEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.CourseId, x.EmployeeId }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
                _ = entity.HasOne(x => x.Course).WithMany(c => c.Enrollments).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<TrainingResultEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.EnrollmentId).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
                _ = entity.HasOne(x => x.Enrollment).WithOne(e => e.Result).HasForeignKey<TrainingResultEntity>(x => x.EnrollmentId).OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureAsset(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<AssetTypeEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code).IsUnique();
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<AssetEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code).IsUnique();
                _ = entity.HasIndex(x => new { x.Status, x.CompanyId });
                _ = entity.HasIndex(x => x.AssetTypeId);
                _ = entity.Property(x => x.Status).HasMaxLength(30);
                _ = entity.HasOne(x => x.AssetType).WithMany(t => t.Assets).HasForeignKey(x => x.AssetTypeId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<AssetTicketEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code).IsUnique();
                _ = entity.HasIndex(x => new { x.Status, x.CompanyId });
                _ = entity.HasIndex(x => x.AssetId);
                _ = entity.Property(x => x.TicketType).HasMaxLength(30);
                _ = entity.Property(x => x.Status).HasMaxLength(30);
                _ = entity.HasOne(x => x.Asset).WithMany(a => a.Tickets).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureSettings(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<ReportScheduleEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
                _ = entity.Property(x => x.Code).HasMaxLength(50);
                _ = entity.Property(x => x.Name).HasMaxLength(200);
                _ = entity.Property(x => x.ReportType).HasMaxLength(40);
                _ = entity.Property(x => x.CronHint).HasMaxLength(40);
                _ = entity.Property(x => x.EmailTo).HasMaxLength(500);
            });

            _ = modelBuilder.Entity<LegalRateConfigEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Year).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
            });

            _ = modelBuilder.Entity<NotificationTemplateEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
                _ = entity.Property(x => x.Code).HasMaxLength(80);
                _ = entity.Property(x => x.Channel).HasMaxLength(20);
                _ = entity.Property(x => x.Subject).HasMaxLength(300);
            });

            _ = modelBuilder.Entity<ApiClientKeyEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.KeyHash).IsUnique();
                _ = entity.HasIndex(x => x.KeyPrefix);
                _ = entity.Property(x => x.Name).HasMaxLength(200);
                _ = entity.Property(x => x.KeyHash).HasMaxLength(128);
                _ = entity.Property(x => x.KeyPrefix).HasMaxLength(20);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<WebhookSubscriptionEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Name);
                _ = entity.Property(x => x.Name).HasMaxLength(200);
                _ = entity.Property(x => x.Url).HasMaxLength(1000);
                _ = entity.Property(x => x.EventTypes).HasMaxLength(1000);
                _ = entity.Property(x => x.Secret).HasMaxLength(200);
            });

            _ = modelBuilder.Entity<SystemRetentionConfigEntity>(entity =>
            {
                _ = entity.Property(x => x.SoftDeleteRetentionDays).HasDefaultValue(365);
            });

            _ = modelBuilder.Entity<SmsGatewayConfigEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Provider);
                _ = entity.Property(x => x.Provider).HasMaxLength(80);
                _ = entity.Property(x => x.ApiUrl).HasMaxLength(1000);
                _ = entity.Property(x => x.ApiKey).HasMaxLength(500);
                _ = entity.Property(x => x.SenderId).HasMaxLength(80);
            });

            _ = modelBuilder.Entity<ZaloOaConfigEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.OaId);
                _ = entity.Property(x => x.OaId).HasMaxLength(80);
                _ = entity.Property(x => x.AppId).HasMaxLength(80);
                _ = entity.Property(x => x.SecretKey).HasMaxLength(500);
                _ = entity.Property(x => x.AccessToken).HasMaxLength(2000);
                _ = entity.Property(x => x.RefreshToken).HasMaxLength(2000);
            });

            _ = modelBuilder.Entity<IpAllowlistEntryEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.CidrOrIp);
                _ = entity.Property(x => x.CidrOrIp).HasMaxLength(100);
            });
        }

        private static void ConfigureWorkflow(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<WorkflowDefinitionEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
                _ = entity.HasIndex(x => new { x.EntityType, x.CompanyId, x.IsActive });
                _ = entity.Property(x => x.Code).HasMaxLength(50);
                _ = entity.Property(x => x.Name).HasMaxLength(200);
                _ = entity.Property(x => x.EntityType).HasMaxLength(40);
                _ = entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<WorkflowStepEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.DefinitionId, x.StepOrder });
                _ = entity.Property(x => x.Name).HasMaxLength(200);
                _ = entity.Property(x => x.ApproverResolver).HasMaxLength(20);
                _ = entity.Property(x => x.RequiredRoleCode).HasMaxLength(50);
                _ = entity.HasOne(x => x.Definition).WithMany(d => d.Steps).HasForeignKey(x => x.DefinitionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<WorkflowInstanceEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.EntityType, x.EntityId });
                _ = entity.HasIndex(x => x.Status);
                _ = entity.Property(x => x.EntityType).HasMaxLength(40);
                _ = entity.Property(x => x.Status).HasMaxLength(20);
                _ = entity.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.DefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<WorkflowTaskEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.InstanceId, x.Status });
                _ = entity.HasIndex(x => x.AssigneeEmployeeId);
                _ = entity.Property(x => x.Status).HasMaxLength(20);
                _ = entity.Property(x => x.Action).HasMaxLength(20);
                _ = entity.HasOne(x => x.Instance).WithMany(i => i.Tasks).HasForeignKey(x => x.InstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<WorkflowFormTemplateEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.EntityType);
                _ = entity.Property(x => x.EntityType).HasMaxLength(40);
                _ = entity.Property(x => x.Name).HasMaxLength(200);
            });
        }

        private static void ConfigureAudit(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<ActionLogEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.CreatedById);
                _ = entity.HasIndex(x => x.EntityId);
            });

            _ = modelBuilder.Entity<NotificationEntity>(entity =>
            {
                _ = entity.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
                _ = entity.HasIndex(x => x.EmployeeId);
                _ = entity.HasIndex(x => x.Type);
            });

            _ = modelBuilder.Entity<DeviceTokenEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.UserId);
                _ = entity.HasIndex(x => x.Token);
            });

            _ = modelBuilder.Entity<NotificationSettingEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.UserId).IsUnique();
            });
        }
    }

    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter()
            : base(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }
}
