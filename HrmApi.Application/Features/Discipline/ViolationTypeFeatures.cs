using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Discipline;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Discipline;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Discipline
{
    public class GetViolationTypesPagedQuery : ViolationTypePagedQuery, IRequest<PagedResult<ViolationTypeDto>> { }

    public class GetViolationTypesPagedQueryHandler : IRequestHandler<GetViolationTypesPagedQuery, PagedResult<ViolationTypeDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetViolationTypesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ViolationTypeDto>> Handle(GetViolationTypesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ViolationTypeEntity> query = _context.ViolationTypeEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.Severity))
            {
                query = query.Where(x => x.Severity == request.Severity.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<ViolationTypeEntity> rows = await query
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            return new PagedResult<ViolationTypeDto>(rows.Select(ToDto).ToList(), total, request.PageIndex, request.PageSize);
        }

        internal static ViolationTypeDto ToDto(ViolationTypeEntity e)
        {
            return new()
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                Severity = e.Severity,
                IsActive = e.IsActive,
                DisplayOrder = e.DisplayOrder,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            };
        }
    }

    public class GetViolationTypeByIdQuery : IdRequest, IRequest<ViolationTypeDto?> { }

    public class GetViolationTypeByIdQueryHandler : IRequestHandler<GetViolationTypeByIdQuery, ViolationTypeDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetViolationTypeByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ViolationTypeDto?> Handle(GetViolationTypeByIdQuery request, CancellationToken cancellationToken)
        {
            ViolationTypeEntity? e = await _context.ViolationTypeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : GetViolationTypesPagedQueryHandler.ToDto(e);
        }
    }

    public class CreateViolationTypeCommand : ViolationTypeCommandFields, IRequest<Guid> { }

    public class CreateViolationTypeCommandHandler : IRequestHandler<CreateViolationTypeCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateViolationTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateViolationTypeCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            string code = request.Code!.Trim().ToUpperInvariant();
            if (await _context.ViolationTypeEntities.AnyAsync(x => !x.IsDeleted && x.Code == code, cancellationToken))
            {
                throw new InvalidOperationException("Mã loại vi phạm đã tồn tại.");
            }

            ViolationTypeEntity entity = new()
            {
                Code = code,
                Name = request.Name!.Trim(),
                Description = request.Description,
                Severity = string.IsNullOrWhiteSpace(request.Severity) ? ViolationSeverity.Medium : request.Severity.Trim().ToUpperInvariant(),
                IsActive = request.IsActive ?? true,
                DisplayOrder = request.DisplayOrder ?? 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.ViolationTypeEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(ViolationTypeCommandFields request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã loại vi phạm là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên loại vi phạm là bắt buộc.");
            }
        }
    }

    public class UpdateViolationTypeCommand : ViolationTypeCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateViolationTypeCommandHandler : IRequestHandler<UpdateViolationTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateViolationTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateViolationTypeCommand request, CancellationToken cancellationToken)
        {
            ViolationTypeEntity? entity = await _context.ViolationTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            CreateViolationTypeCommandHandler.Validate(request);
            string code = request.Code!.Trim().ToUpperInvariant();
            if (await _context.ViolationTypeEntities.AnyAsync(x => !x.IsDeleted && x.Code == code && x.Id != request.Id, cancellationToken))
            {
                throw new InvalidOperationException("Mã loại vi phạm đã tồn tại.");
            }

            entity.Code = code;
            entity.Name = request.Name!.Trim();
            entity.Description = request.Description;
            entity.Severity = string.IsNullOrWhiteSpace(request.Severity) ? entity.Severity : request.Severity.Trim().ToUpperInvariant();
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.DisplayOrder = request.DisplayOrder ?? entity.DisplayOrder;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteViolationTypeCommand : IdRequest, IRequest<bool> { }

    public class DeleteViolationTypeCommandHandler : IRequestHandler<DeleteViolationTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteViolationTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteViolationTypeCommand request, CancellationToken cancellationToken)
        {
            ViolationTypeEntity? entity = await _context.ViolationTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (await _context.ViolationEntities.AnyAsync(x => !x.IsDeleted && x.ViolationTypeId == request.Id, cancellationToken))
            {
                throw new InvalidOperationException("Loại vi phạm đang được dùng — không thể xóa.");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
