using HrmApi.Application.DTOs.Auth;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IPermissionCache
    {
        bool TryGet(Guid userId, out AuthContextDto? context);

        void Set(Guid userId, AuthContextDto context);

        void InvalidateUser(Guid userId);

        Task InvalidateByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        void InvalidateAll();
    }
}
