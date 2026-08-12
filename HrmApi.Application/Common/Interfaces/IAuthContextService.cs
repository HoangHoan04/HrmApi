using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.DTOs.Auth;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IAuthContextService
    {
        Task<AuthContextDto> LoadAuthContextAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
