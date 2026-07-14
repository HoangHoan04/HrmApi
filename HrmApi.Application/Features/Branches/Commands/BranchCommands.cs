using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Features.Branches.Commands
{
    #region Create Command
    public class CreateBranchCommand : IRequest<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string GroupSalary { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
    }

    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateBranchCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            var exists = await _context.BranchEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.ToLower(), cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("Mã chi nhánh đã tồn tại trong hệ thống.");
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                var companyExists = await _context.CompanyEntities.AnyAsync(c => c.Id == request.CompanyId.Value, cancellationToken);
                if (!companyExists)
                {
                    throw new InvalidOperationException("Công ty được chọn không hợp lệ.");
                }
            }

            var branch = new BranchEntity
            {
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                Address = request.Address?.Trim() ?? string.Empty,
                IpAddress = request.IpAddress?.Trim() ?? string.Empty,
                GroupSalary = request.GroupSalary?.Trim() ?? string.Empty,
                ShortName = request.ShortName?.Trim() ?? string.Empty,
                Type = request.Type?.Trim() ?? string.Empty,
                CompanyId = request.CompanyId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.BranchEntities.Add(branch);
            await _context.SaveChangesAsync(cancellationToken);

            return branch.Id;
        }
    }
    #endregion

    #region Update Command
    public class UpdateBranchCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string GroupSalary { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
    }

    public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UpdateBranchCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _context.BranchEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (branch == null) return false;

            var duplicateCode = await _context.BranchEntities
                .AnyAsync(x => x.Id != request.Id && x.Code.ToLower() == request.Code.ToLower(), cancellationToken);

            if (duplicateCode)
            {
                throw new InvalidOperationException("Mã chi nhánh đã tồn tại ở chi nhánh khác.");
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                var companyExists = await _context.CompanyEntities.AnyAsync(c => c.Id == request.CompanyId.Value, cancellationToken);
                if (!companyExists)
                {
                    throw new InvalidOperationException("Công ty được chọn không hợp lệ.");
                }
            }

            branch.Code = request.Code.Trim();
            branch.Name = request.Name.Trim();
            branch.Description = request.Description?.Trim() ?? string.Empty;
            branch.Address = request.Address?.Trim() ?? string.Empty;
            branch.IpAddress = request.IpAddress?.Trim() ?? string.Empty;
            branch.GroupSalary = request.GroupSalary?.Trim() ?? string.Empty;
            branch.ShortName = request.ShortName?.Trim() ?? string.Empty;
            branch.Type = request.Type?.Trim() ?? string.Empty;
            branch.CompanyId = request.CompanyId;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
    #endregion

    #region Activate Command
    public class ActivateBranchCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivateBranchCommandHandler : IRequestHandler<ActivateBranchCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public ActivateBranchCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ActivateBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _context.BranchEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (branch == null) return false;

            branch.IsDeleted = false;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
    #endregion

    #region Deactivate Command
    public class DeactivateBranchCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateBranchCommandHandler : IRequestHandler<DeactivateBranchCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeactivateBranchCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeactivateBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _context.BranchEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (branch == null) return false;

            branch.IsDeleted = true;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
    #endregion
}
