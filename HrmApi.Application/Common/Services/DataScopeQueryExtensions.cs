using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.EmployeeMovement;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Application.Common.Services
{
    public static class DataScopeQueryExtensions
    {
        public static async Task<IQueryable<EmployeeEntity>> ApplyEmployeeDataScopeAsync(
            this IQueryable<EmployeeEntity> query,
            IDataScopeService dataScope,
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            string scope = await dataScope.GetEffectiveScopeAsync(permissionCode, cancellationToken);
            if (scope == DataScopes.All)
            {
                return query;
            }

            DataScopeActor actor = await dataScope.GetActorAsync(cancellationToken);

            return scope switch
            {
                DataScopes.Branch when actor.BranchId.HasValue
                    => query.Where(x => x.BranchId == actor.BranchId),
                DataScopes.Branch when actor.CompanyId.HasValue
                    => query.Where(x => x.CompanyId == actor.CompanyId && x.BranchId == null),
                DataScopes.Department when actor.DepartmentId.HasValue
                    => query.Where(x => x.DepartmentId == actor.DepartmentId),
                DataScopes.Department when actor.BranchId.HasValue
                    => query.Where(x => x.BranchId == actor.BranchId),
                DataScopes.Own when actor.EmployeeId.HasValue
                    => query.Where(x => x.Id == actor.EmployeeId),
                _ => query.Where(_ => false),
            };
        }

        public static async Task<IQueryable<DepartmentEntity>> ApplyDepartmentDataScopeAsync(
            this IQueryable<DepartmentEntity> query,
            IDataScopeService dataScope,
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            string scope = await dataScope.GetEffectiveScopeAsync(permissionCode, cancellationToken);
            if (scope == DataScopes.All)
            {
                return query;
            }

            DataScopeActor actor = await dataScope.GetActorAsync(cancellationToken);

            return scope switch
            {
                DataScopes.Branch when actor.BranchId.HasValue
                    => query.Where(x => x.BranchId == actor.BranchId),
                DataScopes.Branch when actor.CompanyId.HasValue
                    => query.Where(x => x.CompanyId == actor.CompanyId && x.BranchId == null),
                DataScopes.Department when actor.DepartmentId.HasValue
                    => query.Where(x => x.Id == actor.DepartmentId || x.ParentDepartmentId == actor.DepartmentId),
                DataScopes.Own when actor.EmployeeId.HasValue
                    => query.Where(x => x.ManagerId == actor.EmployeeId || x.DeputyManagerId == actor.EmployeeId),
                _ => query.Where(_ => false),
            };
        }

        public static async Task<IQueryable<BranchEntity>> ApplyBranchDataScopeAsync(
            this IQueryable<BranchEntity> query,
            IDataScopeService dataScope,
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            string scope = await dataScope.GetEffectiveScopeAsync(permissionCode, cancellationToken);
            if (scope == DataScopes.All)
            {
                return query;
            }

            DataScopeActor actor = await dataScope.GetActorAsync(cancellationToken);

            return scope switch
            {
                DataScopes.Branch or DataScopes.Department when actor.BranchId.HasValue
                    => query.Where(x => x.Id == actor.BranchId || x.ParentBranchId == actor.BranchId),
                DataScopes.Branch when actor.CompanyId.HasValue && !actor.BranchId.HasValue
                    => query.Where(x => x.CompanyId == actor.CompanyId),
                DataScopes.Own when actor.EmployeeId.HasValue
                    => query.Where(x => x.ManagerId == actor.EmployeeId),
                _ => query.Where(_ => false),
            };
        }

        public static async Task<IQueryable<ContractEntity>> ApplyContractDataScopeAsync(
            this IQueryable<ContractEntity> query,
            IDataScopeService dataScope,
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            string scope = await dataScope.GetEffectiveScopeAsync(permissionCode, cancellationToken);
            if (scope == DataScopes.All)
            {
                return query;
            }

            DataScopeActor actor = await dataScope.GetActorAsync(cancellationToken);

            return scope switch
            {
                DataScopes.Branch when actor.BranchId.HasValue
                    => query.Where(x => x.BranchId == actor.BranchId),
                DataScopes.Branch when actor.CompanyId.HasValue
                    => query.Where(x => x.CompanyId == actor.CompanyId && x.BranchId == null),
                DataScopes.Department when actor.DepartmentId.HasValue
                    => query.Where(x => x.DepartmentId == actor.DepartmentId),
                DataScopes.Department when actor.BranchId.HasValue
                    => query.Where(x => x.BranchId == actor.BranchId),
                DataScopes.Own when actor.EmployeeId.HasValue
                    => query.Where(x => x.EmployeeId == actor.EmployeeId),
                _ => query.Where(_ => false),
            };
        }


        public static async Task<IQueryable<TransferEmployeeEntity>> ApplyTransferEmployeeDataScopeAsync(
            this IQueryable<TransferEmployeeEntity> query,
            IQueryable<EmployeeEntity> employees,
            IDataScopeService dataScope,
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            IQueryable<EmployeeEntity> scopedEmployees = await employees.ApplyEmployeeDataScopeAsync(
                dataScope, permissionCode, cancellationToken);
            return query.Where(x => scopedEmployees.Select(e => e.Id).Contains(x.EmployeeId));
        }
    }
}
