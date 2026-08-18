using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Notification;
using HrmApi.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HrmApi.WebApi.Services
{
    public class NotificationRealtimeService : INotificationRealtimeService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationRealtimeService> _logger;

        public NotificationRealtimeService(IHubContext<NotificationHub> hubContext, ILogger<NotificationRealtimeService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationToUserAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"user_{userId.ToString().ToLowerInvariant()}";
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification, cancellationToken);
                _logger.LogInformation("SignalR notification sent to user group {Group}", groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SignalR notification to user {UserId}", userId);
            }
        }

        public async Task SendNotificationToUsersAsync(IEnumerable<Guid> userIds, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupNames = userIds.Select(id => $"user_{id.ToString().ToLowerInvariant()}").ToList();
                foreach (var group in groupNames)
                {
                    await _hubContext.Clients.Group(group).SendAsync("ReceiveNotification", notification, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SignalR notification to user list");
            }
        }

        public async Task SendBroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.Group("broadcast_all").SendAsync("ReceiveNotification", notification, cancellationToken);
                _logger.LogInformation("SignalR broadcast notification sent to all connected users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SignalR broadcast notification");
            }
        }

        public async Task UpdateUnreadCountAsync(Guid userId, int unreadCount, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"user_{userId.ToString().ToLowerInvariant()}";
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveUnreadCount", unreadCount, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SignalR unread count to user {UserId}", userId);
            }
        }
    }
}
