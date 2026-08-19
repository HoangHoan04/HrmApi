using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.AuditLog;
using HrmApi.Domain.Enums;
using System.Text.Json;

namespace HrmApi.Application.Services
{
    public class ActionLogService : IActionLogService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ActionLogService(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task LogActionAsync(
            ActionType actionType,
            string entityName,
            Guid? entityId,
            object? oldValue,
            object? newValue,
            string? note = null)
        {
            Guid userId = _currentUserService.UserId ?? Guid.Empty;
            string userCode = _currentUserService.UserCode ?? "SYSTEM";
            string username = _currentUserService.Username ?? "SYSTEM";
            string? ip = _currentUserService.IpAddress;
            string? userAgent = _currentUserService.UserAgent;

            string? oldValJson = null;
            string? newValJson = null;

            if (oldValue != null)
            {
                try
                {
                    oldValJson = JsonSerializer.Serialize(oldValue);
                }
                catch { }
            }

            if (newValue != null)
            {
                try
                {
                    newValJson = JsonSerializer.Serialize(newValue);
                }
                catch { }
            }

            var log = new ActionLogEntity
            {
                Id = Guid.NewGuid(),
                CreatedById = userId,
                CreatedByCode = userCode,
                CreatedByName = username,
                CreatedNote = note,
                ActionType = actionType.ToString(),
                EntityId = entityId,
                EntityName = entityName,
                OldValue = oldValJson,
                NewValue = newValJson,
                IpAddress = ip,
                UserAgent = userAgent,
                Location = null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _ = _context.ActionLogEntities.Add(log);
            _ = await _context.SaveChangesAsync(default);
        }
    }
}
