using HrmApi.Domain.Enums;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IActionLogService
    {
        Task LogActionAsync(
            ActionType actionType,
            string entityName,
            Guid? entityId,
            object? oldValue,
            object? newValue,
            string? note = null);
    }
}
