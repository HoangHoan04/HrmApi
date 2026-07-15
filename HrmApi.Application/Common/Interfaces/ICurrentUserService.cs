using System;

namespace HrmApi.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserCode { get; }
        string? Username { get; }
        string? IpAddress { get; }
        string? UserAgent { get; }
    }
}
