using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.DTOs.Training;
using HrmApi.Domain.Entities.Training;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Training
{
    public class GetTrainingCoursesPagedQuery : TrainingCoursePagedQuery, IRequest<PagedResult<TrainingCourseDto>> { }

    public class GetTrainingCoursesPagedQueryHandler : IRequestHandler<GetTrainingCoursesPagedQuery, PagedResult<TrainingCourseDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTrainingCoursesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<TrainingCourseDto>> Handle(GetTrainingCoursesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<TrainingCourseEntity> query = _context.TrainingCourseEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s)
                    || (x.Provider != null && x.Provider.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);
            List<TrainingCourseEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<TrainingCourseDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<TrainingCourseDto>> MapManyAsync(List<TrainingCourseEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> companyIds = rows.Select(x => x.CompanyId).Distinct().ToList();
            List<Guid> branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            Dictionary<Guid, string> companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> branches = branchIds.Count == 0 ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e => new TrainingCourseDto
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                CompanyId = e.CompanyId,
                CompanyName = companies.GetValueOrDefault(e.CompanyId),
                BranchId = e.BranchId,
                BranchName = e.BranchId.HasValue ? branches.GetValueOrDefault(e.BranchId.Value) : null,
                Provider = e.Provider,
                Hours = e.Hours,
                BudgetAmount = e.BudgetAmount,
                Status = e.Status,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            }).ToList();
        }
    }

    public class GetTrainingCourseByIdQuery : IdRequest, IRequest<TrainingCourseDto?> { }

    public class GetTrainingCourseByIdQueryHandler : IRequestHandler<GetTrainingCourseByIdQuery, TrainingCourseDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetTrainingCoursesPagedQueryHandler _mapper;
        public GetTrainingCourseByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetTrainingCoursesPagedQueryHandler(context);
        }

        public async Task<TrainingCourseDto?> Handle(GetTrainingCourseByIdQuery request, CancellationToken cancellationToken)
        {
            TrainingCourseEntity? e = await _context.TrainingCourseEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateTrainingCourseCommand : TrainingCourseCommandFields, IRequest<Guid> { }

    public class CreateTrainingCourseCommandHandler : IRequestHandler<CreateTrainingCourseCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateTrainingCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateTrainingCourseCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            TrainingCourseEntity entity = new()
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                Name = request.Name!.Trim(),
                CompanyId = request.CompanyId!.Value,
                BranchId = request.BranchId,
                Provider = request.Provider,
                Hours = request.Hours ?? 0,
                BudgetAmount = request.BudgetAmount,
                Status = string.IsNullOrWhiteSpace(request.Status) ? TrainingCourseStatus.Draft : request.Status.Trim().ToUpperInvariant(),
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.TrainingCourseEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(TrainingCourseCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã khóa học là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên khóa học là bắt buộc.");
            }

            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
            {
                throw new InvalidOperationException("Công ty là bắt buộc.");
            }

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.TrainingCourseEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
            {
                throw new InvalidOperationException("Mã khóa học đã tồn tại.");
            }

            if (!await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }
        }
    }

    public class UpdateTrainingCourseCommand : TrainingCourseCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateTrainingCourseCommandHandler : IRequestHandler<UpdateTrainingCourseCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateTrainingCourseCommandHandler _create;
        public UpdateTrainingCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateTrainingCourseCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateTrainingCourseCommand request, CancellationToken cancellationToken)
        {
            TrainingCourseEntity? entity = await _context.TrainingCourseEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.Code ??= entity.Code;
            request.Name ??= entity.Name;
            request.CompanyId ??= entity.CompanyId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.Name = request.Name!.Trim();
            entity.CompanyId = request.CompanyId!.Value;
            entity.BranchId = request.BranchId;
            entity.Provider = request.Provider;
            entity.Hours = request.Hours ?? entity.Hours;
            entity.BudgetAmount = request.BudgetAmount;
            entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToUpperInvariant();
            entity.Description = request.Description;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteTrainingCourseCommand : IdRequest, IRequest<bool> { }

    public class DeleteTrainingCourseCommandHandler : IRequestHandler<DeleteTrainingCourseCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteTrainingCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteTrainingCourseCommand request, CancellationToken cancellationToken)
        {
            TrainingCourseEntity? entity = await _context.TrainingCourseEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (await _context.TrainingEnrollmentEntities.AnyAsync(x => !x.IsDeleted && x.CourseId == request.Id, cancellationToken))
            {
                throw new InvalidOperationException("Khóa học đang có đăng ký — không thể xóa.");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
