using System;
using System.Collections.Generic;

namespace HrmApi.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        Guid? EmployeeId { get; }
        Guid? CompanyId { get; }
        Guid? BranchId { get; }
        string? UserCode { get; }
        string? Username { get; }
        string? IpAddress { get; }
        string? UserAgent { get; }

        IReadOnlyList<string> Roles { get; }
        IReadOnlyList<string> Permissions { get; }
        bool IsAdmin { get; }
        bool HasPermission(string code);
    }
}
