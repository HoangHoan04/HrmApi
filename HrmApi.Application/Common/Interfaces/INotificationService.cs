using HrmApi.Application.DTOs.Notification;

namespace HrmApi.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task<Guid> SendAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
        Task<Guid> CreateNotifyAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
        Task<Guid> CreateNotifyForEmployeeAsync(Guid employeeId, string title, string content, string type, string severity, string? targetUrl = null, string? targetType = null, Guid? targetId = null, Guid? senderId = null, CancellationToken cancellationToken = default);
        Task<List<Guid>> NotifyAdminsAsync(string title, string content, string type, string severity, string? targetUrl = null, string? targetType = null, Guid? targetId = null, Guid? companyId = null, Guid? senderId = null, CancellationToken cancellationToken = default);
        Task<List<Guid>> NotifyAdminsAndApproverAsync(Guid? approverEmployeeId, string title, string content, string type, string severity, string? targetUrl = null, string? targetType = null, Guid? targetId = null, Guid? companyId = null, Guid? senderId = null, CancellationToken cancellationToken = default);
        Task<List<Guid>> SendManyAsync(IEnumerable<CreateNotificationDto> dtoList, CancellationToken cancellationToken = default);
        Task<int> SendBroadcastAsync(BroadcastNotificationDto dto, Guid? senderId = null, CancellationToken cancellationToken = default);
        Task RegisterDeviceTokenAsync(Guid userId, Guid? employeeId, RegisterDeviceTokenDto dto, CancellationToken cancellationToken = default);
        Task UnregisterDeviceTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}
