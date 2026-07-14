using HrmApi.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<CompanyEntity> CompanyEntities { get; }
        DbSet<BranchEntity> BranchEntities { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
