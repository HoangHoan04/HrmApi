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
    public class GetTrainingEnrollmentsPagedQuery : TrainingEnrollmentPagedQuery, IRequest<PagedResult<TrainingEnrollmentDto>> { }

    public class GetTrainingEnrollmentsPagedQueryHandler : IRequestHandler<GetTrainingEnrollmentsPagedQuery, PagedResult<TrainingEnrollmentDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTrainingEnrollmentsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<TrainingEnrollmentDto>> Handle(GetTrainingEnrollmentsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<TrainingEnrollmentEntity> query = _context.TrainingEnrollmentEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CourseId.HasValue && request.CourseId != Guid.Empty)
                query = query.Where(x => x.CourseId == request.CourseId);
            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Note != null && x.Note.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<TrainingEnrollmentEntity> rows = await query
                .OrderByDescending(x => x.EnrolledAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<TrainingEnrollmentDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<TrainingEnrollmentDto>> MapManyAsync(List<TrainingEnrollmentEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0) return [];
            var courseIds = rows.Select(x => x.CourseId).Distinct().ToList();
            var empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();

            var courses = await _context.TrainingCourseEntities.AsNoTracking()
                .Where(x => courseIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, x.Name }, cancellationToken);
            var emps = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, Name = x.FullName ?? (x.LastName + " " + x.FirstName).Trim() }, cancellationToken);

            return rows.Select(e =>
            {
                courses.TryGetValue(e.CourseId, out var course);
                emps.TryGetValue(e.EmployeeId, out var emp);
                return new TrainingEnrollmentDto
                {
                    Id = e.Id,
                    CourseId = e.CourseId,
                    CourseCode = course?.Code,
                    CourseName = course?.Name,
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = emp?.Code,
                    EmployeeName = emp?.Name,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status,
                    Note = e.Note,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                };
            }).ToList();
        }
    }

    public class GetTrainingEnrollmentByIdQuery : IdRequest, IRequest<TrainingEnrollmentDto?> { }

    public class GetTrainingEnrollmentByIdQueryHandler : IRequestHandler<GetTrainingEnrollmentByIdQuery, TrainingEnrollmentDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetTrainingEnrollmentsPagedQueryHandler _mapper;
        public GetTrainingEnrollmentByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetTrainingEnrollmentsPagedQueryHandler(context);
        }

        public async Task<TrainingEnrollmentDto?> Handle(GetTrainingEnrollmentByIdQuery request, CancellationToken cancellationToken)
        {
            TrainingEnrollmentEntity? e = await _context.TrainingEnrollmentEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (e == null) return null;
            return (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateTrainingEnrollmentCommand : TrainingEnrollmentCommandFields, IRequest<Guid> { }

    public class CreateTrainingEnrollmentCommandHandler : IRequestHandler<CreateTrainingEnrollmentCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateTrainingEnrollmentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateTrainingEnrollmentCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            TrainingEnrollmentEntity entity = new()
            {
                CourseId = request.CourseId!.Value,
                EmployeeId = request.EmployeeId!.Value,
                EnrolledAt = request.EnrolledAt ?? DateTime.UtcNow,
                Status = string.IsNullOrWhiteSpace(request.Status) ? TrainingEnrollmentStatus.Enrolled : request.Status.Trim().ToUpperInvariant(),
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.TrainingEnrollmentEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(TrainingEnrollmentCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (!request.CourseId.HasValue || request.CourseId == Guid.Empty)
                throw new InvalidOperationException("Khóa học (parent) là bắt buộc.");
            if (!request.EmployeeId.HasValue || request.EmployeeId == Guid.Empty)
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            if (!await _context.TrainingCourseEntities.AnyAsync(x => x.Id == request.CourseId && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Khóa học không tồn tại.");
            if (!await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Nhân viên không tồn tại.");
            if (await _context.TrainingEnrollmentEntities.AnyAsync(
                    x => !x.IsDeleted && x.CourseId == request.CourseId && x.EmployeeId == request.EmployeeId
                         && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
                throw new InvalidOperationException("Nhân viên đã đăng ký khóa học này.");
        }
    }

    public class UpdateTrainingEnrollmentCommand : TrainingEnrollmentCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateTrainingEnrollmentCommandHandler : IRequestHandler<UpdateTrainingEnrollmentCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateTrainingEnrollmentCommandHandler _create;
        public UpdateTrainingEnrollmentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateTrainingEnrollmentCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateTrainingEnrollmentCommand request, CancellationToken cancellationToken)
        {
            TrainingEnrollmentEntity? entity = await _context.TrainingEnrollmentEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            request.CourseId ??= entity.CourseId;
            request.EmployeeId ??= entity.EmployeeId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.CourseId = request.CourseId!.Value;
            entity.EmployeeId = request.EmployeeId!.Value;
            entity.EnrolledAt = request.EnrolledAt ?? entity.EnrolledAt;
            entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToUpperInvariant();
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteTrainingEnrollmentCommand : IdRequest, IRequest<bool> { }

    public class DeleteTrainingEnrollmentCommandHandler : IRequestHandler<DeleteTrainingEnrollmentCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteTrainingEnrollmentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteTrainingEnrollmentCommand request, CancellationToken cancellationToken)
        {
            TrainingEnrollmentEntity? entity = await _context.TrainingEnrollmentEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
