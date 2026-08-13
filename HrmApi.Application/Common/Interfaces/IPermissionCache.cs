using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.DTOs.Auth;

namespace HrmApi.Application.Common.Interfaces
{
    /// <summary>
    /// Cache roles/permissions theo user — Wave B auth optimize.
    /// </summary>
    public interface IPermissionCache
    {
        bool TryGet(Guid userId, out AuthContextDto? context);

        void Set(Guid userId, AuthContextDto context);

        void InvalidateUser(Guid userId);

        /// <summary>Invalidate mọi user đang gán role (khi đổi RolePermission).</summary>
        Task InvalidateByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        void InvalidateAll();
    }
}
