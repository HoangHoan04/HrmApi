using HrmApi.Application.DTOs.Workflow;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IWorkflowEngine
    {

        Task<Guid?> StartAsync(
            string entityType,
            Guid entityId,
            Guid? companyId = null,
            CancellationToken cancellationToken = default);

        Task<bool> AdvanceAsync(
            Guid instanceId,
            bool approve,
            string? note,
            Guid actorUserId,
            Guid? actorEmployeeId = null,
            CancellationToken cancellationToken = default);


        Task<bool> AdvanceForEntityAsync(
            string entityType,
            Guid entityId,
            bool approve,
            string? note,
            Guid actorUserId,
            Guid? actorEmployeeId = null,
            CancellationToken cancellationToken = default);

        Task<List<WorkflowInboxItemDto>> GetInboxAsync(
            Guid employeeId,
            CancellationToken cancellationToken = default);
    }
}
