using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.DTOs.Notification;

namespace HrmApi.Application.Common.Interfaces
{
    public interface INotificationRealtimeService
    {
        Task SendNotificationToUserAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default);
        Task SendNotificationToUsersAsync(IEnumerable<Guid> userIds, NotificationDto notification, CancellationToken cancellationToken = default);
        Task SendBroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);
        Task UpdateUnreadCountAsync(Guid userId, int unreadCount, CancellationToken cancellationToken = default);
    }
}
