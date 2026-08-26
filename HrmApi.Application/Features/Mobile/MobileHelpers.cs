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
            {
                return currentUser.EmployeeId.Value;
            }

            if (currentUser.UserId.HasValue)
            {
                Guid? empId = await context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.UserId == currentUser.UserId.Value && !x.IsDeleted)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                {
                    return empId.Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(currentUser.Username))
            {
                string identifier = currentUser.Username.Trim().ToLowerInvariant();
                Guid? empId = await context.EmployeeEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && ((x.Email != null && x.Email.ToLower() == identifier) || (x.CompanyEmail != null && x.CompanyEmail.ToLower() == identifier) || (x.Code != null && x.Code.ToLower() == identifier)))
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                {
                    return empId.Value;
                }
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }
    }
}
