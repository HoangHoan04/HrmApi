using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMyAnnouncementsQuery : IRequest<List<MobileAnnouncementDto>> { }

    public class GetMyAnnouncementsQueryHandler : IRequestHandler<GetMyAnnouncementsQuery, List<MobileAnnouncementDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyAnnouncementsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<MobileAnnouncementDto>> Handle(GetMyAnnouncementsQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            Guid? companyId = _currentUser.CompanyId;
            if (!companyId.HasValue || companyId == Guid.Empty)
            {
                companyId = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == employeeId)
                    .Select(x => x.CompanyId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (!companyId.HasValue || companyId == Guid.Empty)
                return [];

            DateTime now = DateTime.UtcNow;
            return await _context.CompanyAnnouncementEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.IsActive
                    && x.CompanyId == companyId
                    && x.PublishedAt <= now)
                .OrderByDescending(x => x.PublishedAt)
                .Take(100)
                .Select(x => new MobileAnnouncementDto
                {
                    Id = x.Id,
                    CompanyId = x.CompanyId,
                    Title = x.Title,
                    Body = x.Body,
                    PublishedAt = x.PublishedAt,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                })
                .ToListAsync(cancellationToken);
        }
    }

    public class CreateMobileAnnouncementCommand : CreateMobileAnnouncementRequest, IRequest<Guid> { }

    public class CreateMobileAnnouncementCommandHandler : IRequestHandler<CreateMobileAnnouncementCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CreateMobileAnnouncementCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateMobileAnnouncementCommand request, CancellationToken cancellationToken)
        {
            bool allowed = _currentUser.IsAdmin
                || _currentUser.HasPermission(PermissionCodes.SystemSettingsManage)
                || _currentUser.HasPermission(PermissionCodes.OrgManage);
            if (!allowed)
                throw new InvalidOperationException("Bạn không có quyền tạo thông báo.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new InvalidOperationException("Tiêu đề là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Body))
                throw new InvalidOperationException("Nội dung là bắt buộc.");

            Guid companyId = request.CompanyId ?? _currentUser.CompanyId ?? Guid.Empty;
            if (companyId == Guid.Empty)
            {
                Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
                companyId = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == employeeId)
                    .Select(x => x.CompanyId)
                    .FirstOrDefaultAsync(cancellationToken) ?? Guid.Empty;
            }

            if (companyId == Guid.Empty)
                throw new InvalidOperationException("CompanyId là bắt buộc.");

            var entity = new CompanyAnnouncementEntity
            {
                CompanyId = companyId,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                PublishedAt = request.PublishedAt ?? DateTime.UtcNow,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _context.CompanyAnnouncementEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
}
