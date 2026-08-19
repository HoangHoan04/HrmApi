using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Settings
{
    public class GetLegalRateConfigsPagedQuery : SettingsPagedQuery, IRequest<PagedResult<LegalRateConfigDto>>
    {
        public int? Year { get; set; }
    }

    public class GetLegalRateConfigsPagedQueryHandler : IRequestHandler<GetLegalRateConfigsPagedQuery, PagedResult<LegalRateConfigDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetLegalRateConfigsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<LegalRateConfigDto>> Handle(GetLegalRateConfigsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<LegalRateConfigEntity> query = _context.LegalRateConfigEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.Year.HasValue)
            {
                query = query.Where(x => x.Year == request.Year);
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<LegalRateConfigEntity> rows = await query.OrderByDescending(x => x.Year)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<LegalRateConfigDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static LegalRateConfigDto ToDto(LegalRateConfigEntity e)
        {
            return new()
            {
                Id = e.Id,
                Year = e.Year,
                SocialInsuranceEmployeeRate = e.SocialInsuranceEmployeeRate,
                SocialInsuranceEmployerRate = e.SocialInsuranceEmployerRate,
                HealthInsuranceRate = e.HealthInsuranceRate,
                UnemploymentRate = e.UnemploymentRate,
                PersonalDeduction = e.PersonalDeduction,
                DependentDeduction = e.DependentDeduction,
                Note = e.Note,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            };
        }
    }

    public class GetLegalRateConfigByIdQuery : SettingsIdRequest, IRequest<LegalRateConfigDto?> { }

    public class GetLegalRateConfigByIdQueryHandler : IRequestHandler<GetLegalRateConfigByIdQuery, LegalRateConfigDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetLegalRateConfigByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LegalRateConfigDto?> Handle(GetLegalRateConfigByIdQuery request, CancellationToken cancellationToken)
        {
            LegalRateConfigEntity? e = await _context.LegalRateConfigEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : GetLegalRateConfigsPagedQueryHandler.ToDto(e);
        }
    }

    public class CreateLegalRateConfigCommand : LegalRateConfigCommandFields, IRequest<Guid> { }

    public class CreateLegalRateConfigCommandHandler : IRequestHandler<CreateLegalRateConfigCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateLegalRateConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateLegalRateConfigCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            int year = request.Year!.Value;
            if (await _context.LegalRateConfigEntities.AnyAsync(x => !x.IsDeleted && x.Year == year, cancellationToken))
            {
                throw new InvalidOperationException($"Đã có cấu hình tỷ lệ pháp lý cho năm {year}.");
            }

            LegalRateConfigEntity entity = new()
            {
                Year = year,
                SocialInsuranceEmployeeRate = request.SocialInsuranceEmployeeRate ?? 0,
                SocialInsuranceEmployerRate = request.SocialInsuranceEmployerRate ?? 0,
                HealthInsuranceRate = request.HealthInsuranceRate ?? 0,
                UnemploymentRate = request.UnemploymentRate ?? 0,
                PersonalDeduction = request.PersonalDeduction ?? 0,
                DependentDeduction = request.DependentDeduction ?? 0,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.LegalRateConfigEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(LegalRateConfigCommandFields request)
        {
            if (!request.Year.HasValue || request.Year < 2000 || request.Year > 2100)
            {
                throw new InvalidOperationException("Năm cấu hình không hợp lệ.");
            }
        }
    }

    public class UpdateLegalRateConfigCommand : LegalRateConfigCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateLegalRateConfigCommandHandler : IRequestHandler<UpdateLegalRateConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateLegalRateConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateLegalRateConfigCommand request, CancellationToken cancellationToken)
        {
            LegalRateConfigEntity? entity = await _context.LegalRateConfigEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            CreateLegalRateConfigCommandHandler.Validate(request);
            int year = request.Year!.Value;
            if (await _context.LegalRateConfigEntities.AnyAsync(x => !x.IsDeleted && x.Year == year && x.Id != request.Id, cancellationToken))
            {
                throw new InvalidOperationException($"Đã có cấu hình tỷ lệ pháp lý cho năm {year}.");
            }

            entity.Year = year;
            entity.SocialInsuranceEmployeeRate = request.SocialInsuranceEmployeeRate ?? entity.SocialInsuranceEmployeeRate;
            entity.SocialInsuranceEmployerRate = request.SocialInsuranceEmployerRate ?? entity.SocialInsuranceEmployerRate;
            entity.HealthInsuranceRate = request.HealthInsuranceRate ?? entity.HealthInsuranceRate;
            entity.UnemploymentRate = request.UnemploymentRate ?? entity.UnemploymentRate;
            entity.PersonalDeduction = request.PersonalDeduction ?? entity.PersonalDeduction;
            entity.DependentDeduction = request.DependentDeduction ?? entity.DependentDeduction;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteLegalRateConfigCommand : SettingsIdRequest, IRequest<bool> { }

    public class DeleteLegalRateConfigCommandHandler : IRequestHandler<DeleteLegalRateConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteLegalRateConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteLegalRateConfigCommand request, CancellationToken cancellationToken)
        {
            LegalRateConfigEntity? entity = await _context.LegalRateConfigEntities
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
