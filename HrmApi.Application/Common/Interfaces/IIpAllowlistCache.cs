using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IIpAllowlistCache
    {
        Task<IReadOnlyList<string>> GetActiveEntriesAsync(CancellationToken cancellationToken = default);
        void Invalidate();
    }
}
