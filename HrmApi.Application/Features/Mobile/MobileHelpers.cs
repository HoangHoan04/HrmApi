using HrmApi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    internal static class MobileEmployeeHelper
    {
        public static async Task<Guid> ResolveEmployeeIdAsync(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken)
        {
            if (currentUser.EmployeeId.HasValue && currentUser.EmployeeId != Guid.Empty)
                return currentUser.EmployeeId.Value;

            if (currentUser.UserId.HasValue)
            {
                Guid? empId = await context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                    return empId.Value;
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }
    }
}
