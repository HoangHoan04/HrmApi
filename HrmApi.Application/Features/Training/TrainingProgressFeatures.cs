using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Training;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Training
{
    public class GetTrainingProgressSummaryQuery : TrainingProgressQuery, IRequest<List<TrainingProgressDto>> { }

    public class GetTrainingProgressSummaryQueryHandler : IRequestHandler<GetTrainingProgressSummaryQuery, List<TrainingProgressDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTrainingProgressSummaryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrainingProgressDto>> Handle(GetTrainingProgressSummaryQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Domain.Entities.Training.TrainingCourseEntity> courses = _context.TrainingCourseEntities.AsNoTracking()
                .Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                courses = courses.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.CourseId.HasValue && request.CourseId != Guid.Empty)
            {
                courses = courses.Where(x => x.Id == request.CourseId);
            }

            var courseRows = await courses
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name })
                .ToListAsync(cancellationToken);
            if (courseRows.Count == 0)
            {
                return [];
            }

            List<Guid> courseIds = courseRows.Select(x => x.Id).ToList();
            var enrollments = await _context.TrainingEnrollmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && courseIds.Contains(x.CourseId))
                .GroupBy(x => x.CourseId)
                .Select(g => new
                {
                    CourseId = g.Key,
                    EnrolledCount = g.Count(x => x.Status == TrainingEnrollmentStatus.Enrolled),
                    CompletedCount = g.Count(x => x.Status == TrainingEnrollmentStatus.Completed),
                    DroppedCount = g.Count(x => x.Status == TrainingEnrollmentStatus.Dropped),
                    TotalCount = g.Count(),
                })
                .ToListAsync(cancellationToken);

            var byCourse = enrollments.ToDictionary(x => x.CourseId);
            return courseRows.Select(c =>
            {
                _ = byCourse.TryGetValue(c.Id, out var stats);
                int enrolled = stats?.EnrolledCount ?? 0;
                int completed = stats?.CompletedCount ?? 0;
                int dropped = stats?.DroppedCount ?? 0;
                int total = stats?.TotalCount ?? 0;
                return new TrainingProgressDto
                {
                    CourseId = c.Id,
                    CourseName = c.Name,
                    EnrolledCount = enrolled,
                    CompletedCount = completed,
                    DroppedCount = dropped,
                    CompletionPercent = total == 0 ? 0 : Math.Round(completed * 100m / total, 2),
                };
            }).ToList();
        }
    }
}
