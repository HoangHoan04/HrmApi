using HrmApi.Application.Common.Interfaces;
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
    public class CreateCompanyCommand : IRequest<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? Hotline { get; set; }
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
            var exists = await _context.CompanyEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.ToLower(), cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("Mã công ty đã tồn tại trong hệ thống.");
            }

            var company = new CompanyEntity
            {
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Address = request.Address?.Trim(),
                TaxCode = request.TaxCode?.Trim(),
                Hotline = request.Hotline?.Trim(),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.CompanyEntities.Add(company);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "Company",
                company.Id,
                null,
                new { company.Code, company.Name, company.Description, company.Address, company.TaxCode, company.Hotline },
                "Tạo mới công ty " + company.Name);

            return company.Id;
        }
    }
    #endregion

    #region Update Command
    public class UpdateCompanyCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? Hotline { get; set; }
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

            var duplicateCode = await _context.CompanyEntities
                .AnyAsync(x => x.Id != request.Id && x.Code.ToLower() == request.Code.ToLower(), cancellationToken);

            if (duplicateCode)
            {
                throw new InvalidOperationException("Mã công ty đã tồn tại ở doanh nghiệp khác.");
            }

            var oldValue = new 
            { 
                company.Code, 
                company.Name, 
                company.Description, 
                company.Address, 
                company.TaxCode, 
                company.Hotline 
            };

            company.Code = request.Code.Trim();
            company.Name = request.Name.Trim();
            company.Description = request.Description?.Trim();
            company.Address = request.Address?.Trim();
            company.TaxCode = request.TaxCode?.Trim();
            company.Hotline = request.Hotline?.Trim();
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var newValue = new 
            { 
                company.Code, 
                company.Name, 
                company.Description, 
                company.Address, 
                company.TaxCode, 
                company.Hotline 
            };

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "Company",
                company.Id,
                oldValue,
                newValue,
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
