using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.DTOs.Training;
using HrmApi.Domain.Entities.Training;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Training
{
    public class GetTrainingQuizzesPagedQuery : TrainingQuizPagedQuery, IRequest<PagedResult<TrainingQuizDto>> { }

    public class GetTrainingQuizzesPagedQueryHandler : IRequestHandler<GetTrainingQuizzesPagedQuery, PagedResult<TrainingQuizDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTrainingQuizzesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<TrainingQuizDto>> Handle(GetTrainingQuizzesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<TrainingQuizEntity> query = _context.TrainingQuizEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CourseId.HasValue && request.CourseId != Guid.Empty)
            {
                query = query.Where(x => x.CourseId == request.CourseId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Question.ToLower().Contains(s)
                    || x.OptionA.ToLower().Contains(s)
                    || x.OptionB.ToLower().Contains(s)
                    || (x.OptionC != null && x.OptionC.ToLower().Contains(s))
                    || (x.OptionD != null && x.OptionD.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);
            List<TrainingQuizEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<TrainingQuizDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<TrainingQuizDto>> MapManyAsync(List<TrainingQuizEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> courseIds = rows.Select(x => x.CourseId).Distinct().ToList();
            Dictionary<Guid, string> courses = await _context.TrainingCourseEntities.AsNoTracking()
                .Where(x => courseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e => new TrainingQuizDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                CourseName = courses.GetValueOrDefault(e.CourseId),
                Question = e.Question,
                OptionA = e.OptionA,
                OptionB = e.OptionB,
                OptionC = e.OptionC,
                OptionD = e.OptionD,
                CorrectOption = e.CorrectOption,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            }).ToList();
        }
    }

    public class GetTrainingQuizByIdQuery : IdRequest, IRequest<TrainingQuizDto?> { }

    public class GetTrainingQuizByIdQueryHandler : IRequestHandler<GetTrainingQuizByIdQuery, TrainingQuizDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetTrainingQuizzesPagedQueryHandler _mapper;
        public GetTrainingQuizByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetTrainingQuizzesPagedQueryHandler(context);
        }

        public async Task<TrainingQuizDto?> Handle(GetTrainingQuizByIdQuery request, CancellationToken cancellationToken)
        {
            TrainingQuizEntity? e = await _context.TrainingQuizEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateTrainingQuizCommand : TrainingQuizCommandFields, IRequest<Guid> { }

    public class CreateTrainingQuizCommandHandler : IRequestHandler<CreateTrainingQuizCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateTrainingQuizCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateTrainingQuizCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, cancellationToken);
            string correct = NormalizeCorrectOption(request.CorrectOption!);
            TrainingQuizEntity entity = new()
            {
                CourseId = request.CourseId!.Value,
                Question = request.Question!.Trim(),
                OptionA = request.OptionA!.Trim(),
                OptionB = request.OptionB!.Trim(),
                OptionC = string.IsNullOrWhiteSpace(request.OptionC) ? null : request.OptionC.Trim(),
                OptionD = string.IsNullOrWhiteSpace(request.OptionD) ? null : request.OptionD.Trim(),
                CorrectOption = correct,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.TrainingQuizEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(TrainingQuizCommandFields request, CancellationToken cancellationToken)
        {
            if (!request.CourseId.HasValue || request.CourseId == Guid.Empty)
            {
                throw new InvalidOperationException("Khóa học (parent) là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Question))
            {
                throw new InvalidOperationException("Câu hỏi là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.OptionA))
            {
                throw new InvalidOperationException("Đáp án A là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.OptionB))
            {
                throw new InvalidOperationException("Đáp án B là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.CorrectOption))
            {
                throw new InvalidOperationException("Đáp án đúng là bắt buộc.");
            }

            string correct = NormalizeCorrectOption(request.CorrectOption);
            if (correct is not ("A" or "B" or "C" or "D"))
            {
                throw new InvalidOperationException("Đáp án đúng phải là A, B, C hoặc D.");
            }

            if (correct == "C" && string.IsNullOrWhiteSpace(request.OptionC))
            {
                throw new InvalidOperationException("Đáp án C là bắt buộc khi chọn đúng là C.");
            }

            if (correct == "D" && string.IsNullOrWhiteSpace(request.OptionD))
            {
                throw new InvalidOperationException("Đáp án D là bắt buộc khi chọn đúng là D.");
            }

            if (!await _context.TrainingCourseEntities.AnyAsync(x => x.Id == request.CourseId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Khóa học không tồn tại.");
            }
        }

        internal static string NormalizeCorrectOption(string value)
        {
            return value.Trim().ToUpperInvariant();
        }
    }

    public class UpdateTrainingQuizCommand : TrainingQuizCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateTrainingQuizCommandHandler : IRequestHandler<UpdateTrainingQuizCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateTrainingQuizCommandHandler _create;
        public UpdateTrainingQuizCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateTrainingQuizCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateTrainingQuizCommand request, CancellationToken cancellationToken)
        {
            TrainingQuizEntity? entity = await _context.TrainingQuizEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.CourseId ??= entity.CourseId;
            request.Question ??= entity.Question;
            request.OptionA ??= entity.OptionA;
            request.OptionB ??= entity.OptionB;
            request.CorrectOption ??= entity.CorrectOption;
            await _create.ValidateAsync(request, cancellationToken);

            entity.CourseId = request.CourseId!.Value;
            entity.Question = request.Question!.Trim();
            entity.OptionA = request.OptionA!.Trim();
            entity.OptionB = request.OptionB!.Trim();
            entity.OptionC = string.IsNullOrWhiteSpace(request.OptionC) ? null : request.OptionC.Trim();
            entity.OptionD = string.IsNullOrWhiteSpace(request.OptionD) ? null : request.OptionD.Trim();
            entity.CorrectOption = CreateTrainingQuizCommandHandler.NormalizeCorrectOption(request.CorrectOption!);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteTrainingQuizCommand : IdRequest, IRequest<bool> { }

    public class DeleteTrainingQuizCommandHandler : IRequestHandler<DeleteTrainingQuizCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteTrainingQuizCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteTrainingQuizCommand request, CancellationToken cancellationToken)
        {
            TrainingQuizEntity? entity = await _context.TrainingQuizEntities
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
