using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Organization;
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

        public CreateCompanyCommandHandler(IApplicationDbContext context)
        {
            _context = context;
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


        public UpdateCompanyCommandHandler(IApplicationDbContext context)
        {
            _context = context;
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

            company.Code = request.Code.Trim();
            company.Name = request.Name.Trim();
            company.Description = request.Description?.Trim();
            company.Address = request.Address?.Trim();
            company.TaxCode = request.TaxCode?.Trim();
            company.Hotline = request.Hotline?.Trim();
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
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

        public ActivateCompanyCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ActivateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _context.CompanyEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (company == null) return false;

            company.IsDeleted = false;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
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

        public DeactivateCompanyCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _context.CompanyEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (company == null) return false;

            company.IsDeleted = true;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
    #endregion
}
