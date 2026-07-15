using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Entities.AuditLog;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        /* Organizition */
        DbSet<CompanyEntity> CompanyEntities { get; }
        DbSet<BranchEntity> BranchEntities { get; }
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
