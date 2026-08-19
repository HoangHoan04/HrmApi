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
    public class GetTrainingResultsPagedQuery : TrainingResultPagedQuery, IRequest<PagedResult<TrainingResultDto>> { }

    public class GetTrainingResultsPagedQueryHandler : IRequestHandler<GetTrainingResultsPagedQuery, PagedResult<TrainingResultDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTrainingResultsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<TrainingResultDto>> Handle(GetTrainingResultsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<TrainingResultEntity> query = _context.TrainingResultEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.EnrollmentId.HasValue && request.EnrollmentId != Guid.Empty)
            {
                query = query.Where(x => x.EnrollmentId == request.EnrollmentId);
            }

            if (request.CourseId.HasValue && request.CourseId != Guid.Empty)
            {
                query = from r in query
                        join e in _context.TrainingEnrollmentEntities.AsNoTracking().Where(x => !x.IsDeleted)
                            on r.EnrollmentId equals e.Id
                        where e.CourseId == request.CourseId
                        select r;
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => (x.Grade != null && x.Grade.ToLower().Contains(s))
                    || (x.Note != null && x.Note.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);
            List<TrainingResultEntity> rows = await query
                .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<TrainingResultDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<TrainingResultDto>> MapManyAsync(List<TrainingResultEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> enrollmentIds = rows.Select(x => x.EnrollmentId).Distinct().ToList();
            var enrollments = await _context.TrainingEnrollmentEntities.AsNoTracking()
                .Where(x => enrollmentIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.CourseId, x.EmployeeId }, cancellationToken);

            List<Guid> courseIds = enrollments.Values.Select(x => x.CourseId).Distinct().ToList();
            List<Guid> empIds = enrollments.Values.Select(x => x.EmployeeId).Distinct().ToList();

            Dictionary<Guid, string> courses = await _context.TrainingCourseEntities.AsNoTracking()
                .Where(x => courseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> emps = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.FullName ?? (x.LastName + " " + x.FirstName).Trim(), cancellationToken);

            return rows.Select(e =>
            {
                _ = enrollments.TryGetValue(e.EnrollmentId, out var enr);
                return new TrainingResultDto
                {
                    Id = e.Id,
                    EnrollmentId = e.EnrollmentId,
                    CourseName = enr != null ? courses.GetValueOrDefault(enr.CourseId) : null,
                    EmployeeName = enr != null ? emps.GetValueOrDefault(enr.EmployeeId) : null,
                    Score = e.Score,
                    Grade = e.Grade,
                    CompletedAt = e.CompletedAt,
                    CertificateUrl = e.CertificateUrl,
                    Note = e.Note,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                };
            }).ToList();
        }
    }

    public class GetTrainingResultByIdQuery : IdRequest, IRequest<TrainingResultDto?> { }

    public class GetTrainingResultByIdQueryHandler : IRequestHandler<GetTrainingResultByIdQuery, TrainingResultDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetTrainingResultsPagedQueryHandler _mapper;
        public GetTrainingResultByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetTrainingResultsPagedQueryHandler(context);
        }

        public async Task<TrainingResultDto?> Handle(GetTrainingResultByIdQuery request, CancellationToken cancellationToken)
        {
            TrainingResultEntity? e = await _context.TrainingResultEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateTrainingResultCommand : TrainingResultCommandFields, IRequest<Guid> { }

    public class CreateTrainingResultCommandHandler : IRequestHandler<CreateTrainingResultCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateTrainingResultCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateTrainingResultCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            TrainingResultEntity entity = new()
            {
                EnrollmentId = request.EnrollmentId!.Value,
                Score = request.Score,
                Grade = request.Grade,
                CompletedAt = request.CompletedAt,
                CertificateUrl = request.CertificateUrl,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.TrainingResultEntities.Add(entity);
            await MaybeCompleteEnrollmentAsync(request.EnrollmentId.Value, request.CompletedAt, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(TrainingResultCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (!request.EnrollmentId.HasValue || request.EnrollmentId == Guid.Empty)
            {
                throw new InvalidOperationException("Đăng ký đào tạo (parent) là bắt buộc.");
            }

            if (!await _context.TrainingEnrollmentEntities.AnyAsync(x => x.Id == request.EnrollmentId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Đăng ký đào tạo không tồn tại.");
            }

            if (await _context.TrainingResultEntities.AnyAsync(
                    x => !x.IsDeleted && x.EnrollmentId == request.EnrollmentId && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
            {
                throw new InvalidOperationException("Đăng ký này đã có kết quả đào tạo.");
            }
        }

        internal async Task MaybeCompleteEnrollmentAsync(Guid enrollmentId, DateTime? completedAt, CancellationToken cancellationToken)
        {
            if (!completedAt.HasValue)
            {
                return;
            }

            TrainingEnrollmentEntity? enrollment = await _context.TrainingEnrollmentEntities
                .FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDeleted, cancellationToken);
            if (enrollment == null)
            {
                return;
            }

            enrollment.Status = TrainingEnrollmentStatus.Completed;
            enrollment.UpdatedAt = DateTime.UtcNow;
            enrollment.UpdatedBy = _currentUser.UserId;
        }
    }

    public class UpdateTrainingResultCommand : TrainingResultCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateTrainingResultCommandHandler : IRequestHandler<UpdateTrainingResultCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateTrainingResultCommandHandler _create;
        public UpdateTrainingResultCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateTrainingResultCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateTrainingResultCommand request, CancellationToken cancellationToken)
        {
            TrainingResultEntity? entity = await _context.TrainingResultEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.EnrollmentId ??= entity.EnrollmentId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.EnrollmentId = request.EnrollmentId!.Value;
            entity.Score = request.Score;
            entity.Grade = request.Grade;
            entity.CompletedAt = request.CompletedAt ?? entity.CompletedAt;
            entity.CertificateUrl = request.CertificateUrl;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            await _create.MaybeCompleteEnrollmentAsync(entity.EnrollmentId, entity.CompletedAt, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteTrainingResultCommand : IdRequest, IRequest<bool> { }

    public class DeleteTrainingResultCommandHandler : IRequestHandler<DeleteTrainingResultCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteTrainingResultCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteTrainingResultCommand request, CancellationToken cancellationToken)
        {
            TrainingResultEntity? entity = await _context.TrainingResultEntities
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
