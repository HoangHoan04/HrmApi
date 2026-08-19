using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.DTOs.Training;
using HrmApi.Domain.Entities.Training;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Training
{
    public class GetCourseMaterialsPagedQuery : CourseMaterialPagedQuery, IRequest<PagedResult<CourseMaterialDto>> { }

    public class GetCourseMaterialsPagedQueryHandler : IRequestHandler<GetCourseMaterialsPagedQuery, PagedResult<CourseMaterialDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetCourseMaterialsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CourseMaterialDto>> Handle(GetCourseMaterialsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<TrainingCourseMaterialEntity> query = _context.TrainingCourseMaterialEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CourseId.HasValue && request.CourseId != Guid.Empty)
            {
                query = query.Where(x => x.CourseId == request.CourseId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s) || x.FileUrl.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<TrainingCourseMaterialEntity> rows = await query
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<CourseMaterialDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<CourseMaterialDto>> MapManyAsync(List<TrainingCourseMaterialEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> courseIds = rows.Select(x => x.CourseId).Distinct().ToList();
            Dictionary<Guid, string> courses = await _context.TrainingCourseEntities.AsNoTracking()
                .Where(x => courseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e => new CourseMaterialDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                CourseName = courses.GetValueOrDefault(e.CourseId),
                Name = e.Name,
                FileUrl = e.FileUrl,
                DisplayOrder = e.DisplayOrder,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            }).ToList();
        }
    }

    public class GetCourseMaterialByIdQuery : IdRequest, IRequest<CourseMaterialDto?> { }

    public class GetCourseMaterialByIdQueryHandler : IRequestHandler<GetCourseMaterialByIdQuery, CourseMaterialDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetCourseMaterialsPagedQueryHandler _mapper;
        public GetCourseMaterialByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetCourseMaterialsPagedQueryHandler(context);
        }

        public async Task<CourseMaterialDto?> Handle(GetCourseMaterialByIdQuery request, CancellationToken cancellationToken)
        {
            TrainingCourseMaterialEntity? e = await _context.TrainingCourseMaterialEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateCourseMaterialCommand : CourseMaterialCommandFields, IRequest<Guid> { }

    public class CreateCourseMaterialCommandHandler : IRequestHandler<CreateCourseMaterialCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateCourseMaterialCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateCourseMaterialCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, cancellationToken);
            TrainingCourseMaterialEntity entity = new()
            {
                CourseId = request.CourseId!.Value,
                Name = request.Name!.Trim(),
                FileUrl = request.FileUrl!.Trim(),
                DisplayOrder = request.DisplayOrder ?? 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.TrainingCourseMaterialEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(CourseMaterialCommandFields request, CancellationToken cancellationToken)
        {
            if (!request.CourseId.HasValue || request.CourseId == Guid.Empty)
            {
                throw new InvalidOperationException("Khóa học (parent) là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên tài liệu là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.FileUrl))
            {
                throw new InvalidOperationException("Đường dẫn file là bắt buộc.");
            }

            if (!await _context.TrainingCourseEntities.AnyAsync(x => x.Id == request.CourseId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Khóa học không tồn tại.");
            }
        }
    }

    public class UpdateCourseMaterialCommand : CourseMaterialCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateCourseMaterialCommandHandler : IRequestHandler<UpdateCourseMaterialCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateCourseMaterialCommandHandler _create;
        public UpdateCourseMaterialCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateCourseMaterialCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateCourseMaterialCommand request, CancellationToken cancellationToken)
        {
            TrainingCourseMaterialEntity? entity = await _context.TrainingCourseMaterialEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.CourseId ??= entity.CourseId;
            request.Name ??= entity.Name;
            request.FileUrl ??= entity.FileUrl;
            await _create.ValidateAsync(request, cancellationToken);

            entity.CourseId = request.CourseId!.Value;
            entity.Name = request.Name!.Trim();
            entity.FileUrl = request.FileUrl!.Trim();
            entity.DisplayOrder = request.DisplayOrder ?? entity.DisplayOrder;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteCourseMaterialCommand : IdRequest, IRequest<bool> { }

    public class DeleteCourseMaterialCommandHandler : IRequestHandler<DeleteCourseMaterialCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteCourseMaterialCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteCourseMaterialCommand request, CancellationToken cancellationToken)
        {
            TrainingCourseMaterialEntity? entity = await _context.TrainingCourseMaterialEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
