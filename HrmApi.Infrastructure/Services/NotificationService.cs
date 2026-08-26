using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Notification;
using HrmApi.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HrmApi.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IApplicationDbContext context, IServiceProvider serviceProvider, ILogger<NotificationService> logger)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<Guid> SendAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
        {
            return await CreateNotifyAsync(dto, cancellationToken);
        }

        public async Task<Guid> CreateNotifyAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
        {
            if (dto.UserId == Guid.Empty)
            {
                if (dto.EmployeeId.HasValue && dto.EmployeeId.Value != Guid.Empty)
                {
                    var resolvedUserId = await _context.EmployeeEntities.AsNoTracking()
                        .Where(e => e.Id == dto.EmployeeId.Value && !e.IsDeleted)
                        .Select(e => e.UserId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (resolvedUserId.HasValue && resolvedUserId.Value != Guid.Empty)
                    {
                        dto.UserId = resolvedUserId.Value;
                    }
                    else
                    {
                        dto.UserId = dto.EmployeeId.Value;
                    }
                }

                if (dto.UserId == Guid.Empty)
                {
                    _logger.LogWarning("Cannot send notification: UserId is empty and could not be resolved from EmployeeId.");
                    return Guid.Empty;
                }
            }

            var entity = new NotificationEntity
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                EmployeeId = dto.EmployeeId,
                Title = dto.Title,
                Content = dto.Content,
                Type = dto.Type,
                Severity = dto.Severity,
                TargetUrl = dto.TargetUrl,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                DataJson = dto.DataJson,
                IsRead = false,
                IsBroadcast = false,
                SenderId = dto.SenderId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.SenderId ?? Guid.Empty,
            };

            _context.NotificationEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await TryPublishRealtimeAsync(entity, cancellationToken);

            return entity.Id;
        }

        public async Task<Guid> CreateNotifyForEmployeeAsync(
            Guid employeeId,
            string title,
            string content,
            string type,
            string severity,
            string? targetUrl = null,
            string? targetType = null,
            Guid? targetId = null,
            Guid? senderId = null,
            CancellationToken cancellationToken = default)
        {
            var employee = await _context.EmployeeEntities.AsNoTracking()
                .Where(e => e.Id == employeeId && !e.IsDeleted)
                .Select(e => new { e.Id, e.UserId })
                .FirstOrDefaultAsync(cancellationToken);

            if (employee == null)
            {
                _logger.LogWarning("Cannot send notification to employee {EmployeeId}: employee not found.", employeeId);
                return Guid.Empty;
            }

            return await CreateNotifyAsync(new CreateNotificationDto
            {
                UserId = employee.UserId ?? employee.Id,
                EmployeeId = employeeId,
                Title = title,
                Content = content,
                Type = type,
                Severity = severity,
                TargetUrl = targetUrl,
                TargetType = targetType,
                TargetId = targetId,
                SenderId = senderId,
            }, cancellationToken);
        }

        public async Task<List<Guid>> NotifyAdminsAsync(
            string title,
            string content,
            string type,
            string severity,
            string? targetUrl = null,
            string? targetType = null,
            Guid? targetId = null,
            Guid? companyId = null,
            Guid? senderId = null,
            CancellationToken cancellationToken = default)
        {
            var adminRoleCodes = new[] { RoleCodes.Admin, RoleCodes.Hr };

            var adminRolesQuery = from ur in _context.UserRoleEntities.AsNoTracking()
                                  join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                                  where !ur.IsDeleted && !r.IsDeleted && r.IsActive && adminRoleCodes.Contains(r.Code)
                                  select new { ur.UserId, ur.EmployeeId };

            var adminUserRoles = await adminRolesQuery.ToListAsync(cancellationToken);

            var targetList = new List<(Guid UserId, Guid? EmployeeId)>();
            var seen = new HashSet<Guid>();

            foreach (var row in adminUserRoles)
            {
                Guid id = row.UserId.HasValue && row.UserId.Value != Guid.Empty ? row.UserId.Value : (row.EmployeeId ?? Guid.Empty);
                if (id != Guid.Empty && (senderId == null || id != senderId.Value) && seen.Add(id))
                {
                    targetList.Add((id, row.EmployeeId));
                }
            }

            var dtoList = targetList.Select(u => new CreateNotificationDto
            {
                UserId = u.UserId,
                EmployeeId = u.EmployeeId,
                Title = title,
                Content = content,
                Type = type,
                Severity = severity,
                TargetUrl = targetUrl,
                TargetType = targetType,
                TargetId = targetId,
                SenderId = senderId,
            }).ToList();

            return await SendManyAsync(dtoList, cancellationToken);
        }

        public async Task<List<Guid>> NotifyAdminsAndApproverAsync(
            Guid? approverEmployeeId,
            string title,
            string content,
            string type,
            string severity,
            string? targetUrl = null,
            string? targetType = null,
            Guid? targetId = null,
            Guid? companyId = null,
            Guid? senderId = null,
            CancellationToken cancellationToken = default)
        {
            var userIdsToNotify = new HashSet<Guid>();
            var targetUsers = new List<(Guid UserId, Guid? EmployeeId)>();

            // 1. Resolve Approver
            if (approverEmployeeId.HasValue && approverEmployeeId.Value != Guid.Empty)
            {
                var approverEmp = await _context.EmployeeEntities.AsNoTracking()
                    .Where(e => e.Id == approverEmployeeId.Value && !e.IsDeleted)
                    .Select(e => new { e.Id, e.UserId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (approverEmp != null)
                {
                    Guid approverId = approverEmp.UserId ?? approverEmp.Id;
                    if (senderId == null || approverId != senderId.Value)
                    {
                        if (userIdsToNotify.Add(approverId))
                        {
                            targetUsers.Add((approverId, approverEmp.Id));
                        }
                    }
                }
            }

            // 2. Resolve Admins / HRs
            var adminRoleCodes = new[] { RoleCodes.Admin, RoleCodes.Hr };

            var adminRolesQuery = from ur in _context.UserRoleEntities.AsNoTracking()
                                  join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                                  where !ur.IsDeleted && !r.IsDeleted && r.IsActive && adminRoleCodes.Contains(r.Code)
                                  select new { ur.UserId, ur.EmployeeId };

            var adminUserRoles = await adminRolesQuery.ToListAsync(cancellationToken);

            foreach (var a in adminUserRoles)
            {
                Guid id = a.UserId.HasValue && a.UserId.Value != Guid.Empty ? a.UserId.Value : (a.EmployeeId ?? Guid.Empty);
                if (id != Guid.Empty && (senderId == null || id != senderId.Value) && userIdsToNotify.Add(id))
                {
                    targetUsers.Add((id, a.EmployeeId));
                }
            }

            var dtoList = targetUsers.Select(t => new CreateNotificationDto
            {
                UserId = t.UserId,
                EmployeeId = t.EmployeeId,
                Title = title,
                Content = content,
                Type = type,
                Severity = severity,
                TargetUrl = targetUrl,
                TargetType = targetType,
                TargetId = targetId,
                SenderId = senderId,
            }).ToList();

            return await SendManyAsync(dtoList, cancellationToken);
        }

        public async Task<List<Guid>> SendManyAsync(IEnumerable<CreateNotificationDto> dtoList, CancellationToken cancellationToken = default)
        {
            if (dtoList == null) return new List<Guid>();

            var entities = new List<NotificationEntity>();
            foreach (var dto in dtoList)
            {
                if (dto.UserId == Guid.Empty)
                {
                    if (dto.EmployeeId.HasValue && dto.EmployeeId.Value != Guid.Empty)
                    {
                        dto.UserId = dto.EmployeeId.Value;
                    }
                    else
                    {
                        continue;
                    }
                }

                entities.Add(new NotificationEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    EmployeeId = dto.EmployeeId,
                    Title = dto.Title,
                    Content = dto.Content,
                    Type = dto.Type,
                    Severity = dto.Severity,
                    TargetUrl = dto.TargetUrl,
                    TargetType = dto.TargetType,
                    TargetId = dto.TargetId,
                    DataJson = dto.DataJson,
                    IsRead = false,
                    IsBroadcast = false,
                    SenderId = dto.SenderId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.SenderId ?? Guid.Empty,
                });
            }

            if (entities.Count > 0)
            {
                _context.NotificationEntities.AddRange(entities);
                _ = await _context.SaveChangesAsync(cancellationToken);

                foreach (var entity in entities)
                {
                    await TryPublishRealtimeAsync(entity, cancellationToken);
                }
            }

            return entities.Select(e => e.Id).ToList();
        }

        public async Task<int> SendBroadcastAsync(BroadcastNotificationDto dto, Guid? senderId = null, CancellationToken cancellationToken = default)
        {
            var empQuery = _context.EmployeeEntities.AsNoTracking().Where(e => !e.IsDeleted);

            if (dto.CompanyId.HasValue) empQuery = empQuery.Where(e => e.CompanyId == dto.CompanyId);
            if (dto.BranchId.HasValue) empQuery = empQuery.Where(e => e.BranchId == dto.BranchId);
            if (dto.DepartmentId.HasValue) empQuery = empQuery.Where(e => e.DepartmentId == dto.DepartmentId);

            if (dto.TargetUserIds != null && dto.TargetUserIds.Count > 0)
            {
                empQuery = empQuery.Where(e => (e.UserId.HasValue && dto.TargetUserIds.Contains(e.UserId.Value)) || dto.TargetUserIds.Contains(e.Id));
            }

            var recipients = await empQuery.Select(e => new { Id = e.UserId ?? e.Id, EmployeeId = (Guid?)e.Id }).ToListAsync(cancellationToken);

            var notifications = recipients.Select(r => new NotificationEntity
            {
                Id = Guid.NewGuid(),
                UserId = r.Id,
                EmployeeId = r.EmployeeId,
                Title = dto.Title,
                Content = dto.Content,
                Type = dto.Type,
                Severity = dto.Severity,
                TargetUrl = dto.TargetUrl,
                IsRead = false,
                IsBroadcast = true,
                SenderId = senderId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = senderId ?? Guid.Empty,
            }).ToList();

            if (notifications.Count > 0)
            {
                _context.NotificationEntities.AddRange(notifications);
                _ = await _context.SaveChangesAsync(cancellationToken);

                var first = notifications.First();
                await TryPublishRealtimeBroadcastAsync(new NotificationDto
                {
                    Id = first.Id,
                    Title = first.Title,
                    Content = first.Content,
                    Type = first.Type,
                    Severity = first.Severity,
                    TargetUrl = first.TargetUrl,
                    IsRead = false,
                    IsBroadcast = true,
                    CreatedAt = first.CreatedAt,
                }, cancellationToken);
            }

            _logger.LogInformation("Broadcast notification sent to {Count} users: {Title}", notifications.Count, dto.Title);
            return notifications.Count;
        }

        private async Task TryPublishRealtimeAsync(NotificationEntity entity, CancellationToken cancellationToken)
        {
            try
            {
                var realtime = _serviceProvider.GetService<INotificationRealtimeService>();
                if (realtime != null)
                {
                    var dto = new NotificationDto
                    {
                        Id = entity.Id,
                        UserId = entity.UserId,
                        EmployeeId = entity.EmployeeId,
                        Title = entity.Title,
                        Content = entity.Content,
                        Type = entity.Type,
                        Severity = entity.Severity,
                        TargetUrl = entity.TargetUrl,
                        TargetType = entity.TargetType,
                        TargetId = entity.TargetId,
                        DataJson = entity.DataJson,
                        IsRead = entity.IsRead,
                        IsBroadcast = entity.IsBroadcast,
                        SenderId = entity.SenderId,
                        CreatedAt = entity.CreatedAt,
                    };
                    await realtime.SendNotificationToUserAsync(entity.UserId, dto, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish realtime notification for user {UserId}", entity.UserId);
            }
        }

        private async Task TryPublishRealtimeBroadcastAsync(NotificationDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var realtime = _serviceProvider.GetService<INotificationRealtimeService>();
                if (realtime != null)
                {
                    await realtime.SendBroadcastNotificationAsync(dto, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish realtime broadcast notification");
            }
        }

        public async Task RegisterDeviceTokenAsync(Guid userId, Guid? employeeId, RegisterDeviceTokenDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Token)) return;

            var existing = await _context.DeviceTokenEntities
                .FirstOrDefaultAsync(x => x.Token == dto.Token && !x.IsDeleted, cancellationToken);

            if (existing != null)
            {
                existing.UserId = userId;
                existing.EmployeeId = employeeId ?? existing.EmployeeId;
                existing.Platform = dto.Platform;
                existing.DeviceId = dto.DeviceId ?? existing.DeviceId;
                existing.DeviceName = dto.DeviceName ?? existing.DeviceName;
                existing.LastActiveAt = DateTime.UtcNow;
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var tokenEntity = new DeviceTokenEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EmployeeId = employeeId,
                    Token = dto.Token,
                    Platform = dto.Platform,
                    DeviceId = dto.DeviceId,
                    DeviceName = dto.DeviceName,
                    LastActiveAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                };
                _ = _context.DeviceTokenEntities.Add(tokenEntity);
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UnregisterDeviceTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token)) return;

            var existing = await _context.DeviceTokenEntities
                .FirstOrDefaultAsync(x => x.Token == token && !x.IsDeleted, cancellationToken);

            if (existing != null)
            {
                existing.IsActive = false;
                existing.UpdatedAt = DateTime.UtcNow;
                _ = await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
