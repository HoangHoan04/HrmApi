namespace HrmApi.Application.Common.Interfaces
{
    public sealed record DataScopeActor(
        Guid? EmployeeId,
        Guid? CompanyId,
        Guid? BranchId,
        Guid? DepartmentId);

    public interface IDataScopeService
    {

        Task<string> GetEffectiveScopeAsync(string permissionCode, CancellationToken cancellationToken = default);

        Task<DataScopeActor> GetActorAsync(CancellationToken cancellationToken = default);
    }
}
