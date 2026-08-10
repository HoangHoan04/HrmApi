using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.AuditLog;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.EmployeeMovement;
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

        /* Organization */
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

        /* Employee Movement */
        public DbSet<TransferEmployeeEntity> TransferEmployeeEntities { get; set; }
        public DbSet<TransferEmployeePositionEntity> TransferEmployeePositionEntities { get; set; }

        /* Contract */
        public DbSet<ContractTypeEntity> ContractTypeEntities { get; set; }
        public DbSet<ContractEntity> ContractEntities { get; set; }
        public DbSet<ReviewRenewalEntity> ReviewRenewalEntities { get; set; }

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
            ConfigureOrganization(modelBuilder);
            ConfigureEmployee(modelBuilder);
            ConfigureEmployeeMovement(modelBuilder);
            ConfigureContract(modelBuilder);
            ConfigurePermission(modelBuilder);
            ConfigureTimekeeping(modelBuilder);
            ConfigureLeave(modelBuilder);
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

                // Chuẩn mặc định của công ty (FK trên Company)
                _ = entity.HasOne(c => c.TimeKeepingStandard)
                    .WithMany()
                    .HasForeignKey(c => c.TimeKeepingStandardId)
                    .OnDelete(DeleteBehavior.Restrict);
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
                // Đơn/quyết định điều chuyển thuộc về 1 nhân viên — xóa nhân viên thì xóa luôn lịch sử điều chuyển.
                _ = entity.HasOne(t => t.Employee)
                    .WithMany(e => e.TransferEmployees)
                    .HasForeignKey(t => t.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasIndex(t => new { t.EmployeeId, t.EffectiveDate });
            });

            _ = modelBuilder.Entity<TransferEmployeePositionEntity>(entity =>
            {
                // Chi tiết thay đổi là con của đơn điều chuyển — xóa đơn thì xóa luôn chi tiết.
                _ = entity.HasOne(d => d.TransferEmployee)
                    .WithMany(t => t.TransferDetails)
                    .HasForeignKey(d => d.TransferEmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Denormalize EmployeeId để truy vấn nhanh, không cascade theo Employee (đã cascade qua TransferEmployee).
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
                // CompanyEntity chưa có collection riêng cho ContractType nên dùng WithMany() ẩn danh.
                _ = entity.HasOne(t => t.Company)
                    .WithMany()
                    .HasForeignKey(t => t.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasIndex(t => t.Code).IsUnique();
            });

            _ = modelBuilder.Entity<ContractEntity>(entity =>
            {
                // Hợp đồng thuộc về 1 nhân viên — xóa nhân viên thì xóa luôn hồ sơ hợp đồng.
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

                _ = entity.HasOne(c => c.Position)
                    .WithMany()
                    .HasForeignKey(c => c.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Self-reference: hợp đồng tái ký/gia hạn trỏ về hợp đồng trước đó.
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
                // Đợt đánh giá/gia hạn là con của hợp đồng — xóa hợp đồng thì xóa luôn lịch sử đánh giá.
                _ = entity.HasOne(r => r.Contract)
                    .WithMany(c => c.ReviewRenewals)
                    .HasForeignKey(r => r.ContractId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Denormalize EmployeeId để truy vấn nhanh, không cascade theo Employee (đã cascade qua Contract).
                _ = entity.HasOne(r => r.Employee)
                    .WithMany()
                    .HasForeignKey(r => r.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(r => r.ProposedContractType)
                    .WithMany()
                    .HasForeignKey(r => r.ProposedContractTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Hợp đồng mới sinh ra sau khi đề xuất được duyệt — không cascade để tránh xóa hợp đồng khi xóa đánh giá.
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
                _ = entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                _ = entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Restrict);
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
            });
        }

        private static void ConfigureTimekeeping(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<TimeKeepingStandardEntity>(entity =>
            {
                // Danh sách chuẩn thuộc công ty (FK trên TimeKeepingStandard)
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
        }

        private static void ConfigureLeave(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<DayOffConfigEntity>(entity =>
            {
                _ = entity.Property(e => e.DayOffType).HasConversion<string>();

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
                _ = entity.Property(e => e.DayOffType).HasConversion<string>();
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

        private static void ConfigureAudit(ModelBuilder modelBuilder)
        {
            // ActionLog lưu snapshot CreatedById/Code/Name — không tạo FK tới User để tránh ràng buộc cứng.
            _ = modelBuilder.Entity<ActionLogEntity>(entity =>
            {
                _ = entity.HasIndex(x => x.CreatedById);
                _ = entity.HasIndex(x => x.EntityId);
            });
        }
    }
}