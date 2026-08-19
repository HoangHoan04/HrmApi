using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.DTOs.Training;
using HrmApi.Domain.Entities.Training;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMyTrainingCoursesQuery : IRequest<List<TrainingCourseDto>> { }

    public class GetMyTrainingCoursesQueryHandler : IRequestHandler<GetMyTrainingCoursesQuery, List<TrainingCourseDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyTrainingCoursesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<TrainingCourseDto>> Handle(GetMyTrainingCoursesQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            Guid? companyId = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == employeeId)
                .Select(x => x.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            List<Guid> enrolledCourseIds = await _context.TrainingEnrollmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId)
                .Select(x => x.CourseId)
                .Distinct()
                .ToListAsync(cancellationToken);

            IQueryable<TrainingCourseEntity> query = _context.TrainingCourseEntities.AsNoTracking()
                .Where(x => !x.IsDeleted);
            if (enrolledCourseIds.Count > 0)
            {
                query = query.Where(x => enrolledCourseIds.Contains(x.Id)
                    || (companyId.HasValue && x.CompanyId == companyId));
            }
            else if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == companyId);
            }
            else
            {
                return [];
            }

            List<TrainingCourseEntity> rows = await query.OrderBy(x => x.Name).Take(200).ToListAsync(cancellationToken);
            return rows.Select(e => new TrainingCourseDto
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                CompanyId = e.CompanyId,
                BranchId = e.BranchId,
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

    public class GetMyTrainingEnrollmentsQuery : IRequest<List<TrainingEnrollmentDto>> { }

    public class GetMyTrainingEnrollmentsQueryHandler : IRequestHandler<GetMyTrainingEnrollmentsQuery, List<TrainingEnrollmentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyTrainingEnrollmentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<TrainingEnrollmentDto>> Handle(GetMyTrainingEnrollmentsQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            List<TrainingEnrollmentEntity> rows = await _context.TrainingEnrollmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId)
                .OrderByDescending(x => x.EnrolledAt)
                .Take(200)
                .ToListAsync(cancellationToken);
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> courseIds = rows.Select(x => x.CourseId).Distinct().ToList();
            var courses = await _context.TrainingCourseEntities.AsNoTracking()
                .Where(x => courseIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, x.Name }, cancellationToken);
            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == employeeId)
                .Select(x => new { x.Code, Name = x.FullName ?? (x.LastName + " " + x.FirstName).Trim() })
                .FirstOrDefaultAsync(cancellationToken);

            return rows.Select(e =>
            {
                _ = courses.TryGetValue(e.CourseId, out var c);
                return new TrainingEnrollmentDto
                {
                    Id = e.Id,
                    CourseId = e.CourseId,
                    CourseCode = c?.Code,
                    CourseName = c?.Name,
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

    public class GetMyTrainingResultsQuery : IRequest<List<TrainingResultDto>> { }

    public class GetMyTrainingResultsQueryHandler : IRequestHandler<GetMyTrainingResultsQuery, List<TrainingResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyTrainingResultsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<TrainingResultDto>> Handle(GetMyTrainingResultsQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            List<Guid> enrollmentIds = await _context.TrainingEnrollmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            if (enrollmentIds.Count == 0)
            {
                return [];
            }

            List<TrainingResultEntity> rows = await _context.TrainingResultEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && enrollmentIds.Contains(x.EnrollmentId))
                .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                .Take(200)
                .ToListAsync(cancellationToken);
            if (rows.Count == 0)
            {
                return [];
            }

            Dictionary<Guid, Guid> enrollments = await _context.TrainingEnrollmentEntities.AsNoTracking()
                .Where(x => enrollmentIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.CourseId, cancellationToken);
            List<Guid> courseIds = enrollments.Values.Distinct().ToList();
            Dictionary<Guid, string> courses = await _context.TrainingCourseEntities.AsNoTracking()
                .Where(x => courseIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var empName = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == employeeId)
                .Select(x => x.FullName ?? (x.LastName + " " + x.FirstName).Trim())
                .FirstOrDefaultAsync(cancellationToken);

            return rows.Select(e =>
            {
                _ = enrollments.TryGetValue(e.EnrollmentId, out Guid courseId);
                return new TrainingResultDto
                {
                    Id = e.Id,
                    EnrollmentId = e.EnrollmentId,
                    CourseName = courses.GetValueOrDefault(courseId),
                    EmployeeName = empName,
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

    public class GetMobileQuizzesQuery : MobileQuizzesQuery, IRequest<List<MobileQuizQuestionDto>> { }

    public class GetMobileQuizzesQueryHandler : IRequestHandler<GetMobileQuizzesQuery, List<MobileQuizQuestionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMobileQuizzesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<MobileQuizQuestionDto>> Handle(GetMobileQuizzesQuery request, CancellationToken cancellationToken)
        {
            _ = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            return request.CourseId == Guid.Empty
                ? throw new InvalidOperationException("CourseId là bắt buộc.")
                : await _context.TrainingQuizEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CourseId == request.CourseId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new MobileQuizQuestionDto
                {
                    Id = x.Id,
                    CourseId = x.CourseId,
                    Question = x.Question,
                    OptionA = x.OptionA,
                    OptionB = x.OptionB,
                    OptionC = x.OptionC,
                    OptionD = x.OptionD,
                })
                .ToListAsync(cancellationToken);
        }
    }

    public class SubmitMobileQuizCommand : MobileSubmitQuizRequest, IRequest<MobileSubmitQuizResultDto> { }

    public class SubmitMobileQuizCommandHandler : IRequestHandler<SubmitMobileQuizCommand, MobileSubmitQuizResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public SubmitMobileQuizCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<MobileSubmitQuizResultDto> Handle(SubmitMobileQuizCommand request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            if (request.CourseId == Guid.Empty)
            {
                throw new InvalidOperationException("CourseId là bắt buộc.");
            }

            List<TrainingQuizEntity> quizzes = await _context.TrainingQuizEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CourseId == request.CourseId)
                .ToListAsync(cancellationToken);

            int correct = 0;
            foreach (MobileQuizAnswerDto answer in request.Answers ?? [])
            {
                TrainingQuizEntity? q = quizzes.FirstOrDefault(x => x.Id == answer.QuizId);
                if (q == null)
                {
                    continue;
                }

                if (string.Equals(q.CorrectOption?.Trim(), answer.SelectedOption?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    correct++;
                }
            }

            int total = quizzes.Count;
            decimal percent = total == 0 ? 0 : Math.Round(100m * correct / total, 2);

            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                TrainingEnrollmentEntity? enrollment = await _context.TrainingEnrollmentEntities
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.EmployeeId == employeeId && x.CourseId == request.CourseId, cancellationToken);
                if (enrollment != null)
                {
                    enrollment.Note = string.IsNullOrWhiteSpace(enrollment.Note)
                        ? $"[Quiz {percent}%] {request.Note.Trim()}"
                        : enrollment.Note + $" | [Quiz {percent}%] {request.Note.Trim()}";
                    enrollment.UpdatedAt = DateTime.UtcNow;
                    enrollment.UpdatedBy = _currentUser.UserId;
                    _ = await _context.SaveChangesAsync(cancellationToken);
                }
            }

            return new MobileSubmitQuizResultDto
            {
                CourseId = request.CourseId,
                TotalQuestions = total,
                CorrectCount = correct,
                ScorePercent = percent,
                Note = request.Note,
            };
        }
    }
}
