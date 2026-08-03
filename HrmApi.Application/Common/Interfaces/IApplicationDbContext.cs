using System.Threading;
using System.Threading.Tasks;
using HrmApi.Domain.Entities.AuditLog;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Entities.Permission;
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
