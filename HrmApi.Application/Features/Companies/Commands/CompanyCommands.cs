using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Features.Companies.Commands
{
    #region Create Command
    public class CreateCompanyCommand : CompanyCommandFields, IRequest<Guid>
    {
    }

    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateCompanyCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            var company = new CompanyEntity
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            CompanyMapper.ApplyCommandFields(company, request);

            _context.CompanyEntities.Add(company);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "Company",
                company.Id,
                null,
                CompanyMapper.ToLogObject(company),
                "Tạo mới công ty " + company.Name);

            return company.Id;
        }

        internal static async Task ValidateAsync(
            CompanyCommandFields request,
            Guid? excludeId,
            CancellationToken cancellationToken,
            IApplicationDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new InvalidOperationException("Mã công ty là bắt buộc.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên công ty là bắt buộc.");

            var exists = await context.CompanyEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

            if (exists)
                throw new InvalidOperationException("Mã công ty đã tồn tại trong hệ thống.");

            if (request.ParentId.HasValue)
            {
                if (excludeId.HasValue && request.ParentId.Value == excludeId.Value)
                    throw new InvalidOperationException("Công ty không thể là công ty mẹ của chính nó.");

                var parentExists = await context.CompanyEntities
                    .AnyAsync(x => x.Id == request.ParentId.Value, cancellationToken);

                if (!parentExists)
                    throw new InvalidOperationException("Công ty mẹ không tồn tại.");
            }
        }
    }
    #endregion

    #region Update Command
    public class UpdateCompanyCommand : CompanyCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateCompanyCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _context.CompanyEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (company == null) return false;

            await CreateCompanyCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            var oldValue = CompanyMapper.ToLogObject(company);

            CompanyMapper.ApplyCommandFields(company, request);
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "Company",
                company.Id,
                oldValue,
                CompanyMapper.ToLogObject(company),
                "Cập nhật thông tin công ty " + company.Name);

            return true;
        }
    }
    #endregion

    #region Activate Command
    public class ActivateCompanyCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivateCompanyCommandHandler : IRequestHandler<ActivateCompanyCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivateCompanyCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _context.CompanyEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (company == null) return false;

            company.IsDeleted = false;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "Company",
                company.Id,
                new { IsDeleted = true },
                new { IsDeleted = false },
                "Kích hoạt công ty " + company.Name);

            return true;
        }
    }
    #endregion

    #region Deactivate Command
    public class DeactivateCompanyCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateCompanyCommandHandler : IRequestHandler<DeactivateCompanyCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivateCompanyCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _context.CompanyEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (company == null) return false;

            company.IsDeleted = true;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "Company",
                company.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Khóa công ty " + company.Name);

            return true;
        }
    }
    #endregion
}
