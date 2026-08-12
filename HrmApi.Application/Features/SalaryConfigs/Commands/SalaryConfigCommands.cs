using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.SalaryConfigs.Commands
{
    public class CreateSalaryConfigCommand : SalaryConfigCommandFields, IRequest<Guid> { }

    public class CreateSalaryConfigCommandHandler : IRequestHandler<CreateSalaryConfigCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateSalaryConfigCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateSalaryConfigCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            SalaryConfigEntity entity = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                IsActive = request.IsActive ?? true,
                DisplayOrder = request.DisplayOrder ?? 0,
                StandardWorkingDays = request.StandardWorkingDays ?? 26,
                BhxhEmployeeRate = request.BhxhEmployeeRate ?? 8m,
                BhytEmployeeRate = request.BhytEmployeeRate ?? 1.5m,
                BhtnEmployeeRate = request.BhtnEmployeeRate ?? 1m,
                DefaultPayDay = request.DefaultPayDay ?? 5,
                Currency = "VND",
            };
            SalaryConfigMapper.ApplyCommandFields(entity, request);
            _ = _context.SalaryConfigEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "SalaryConfigEntity",
                entity.Id,
                null,
                SalaryConfigMapper.ToLogObject(entity),
                "Tạo cấu hình lương " + entity.Name);
            return entity.Id;
        }

        internal async Task ValidateAsync(SalaryConfigCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new InvalidOperationException("Mã cấu hình lương là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên cấu hình lương là bắt buộc.");

            bool exists = await _context.SalaryConfigEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == request.Code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
                throw new InvalidOperationException("Mã cấu hình lương đã tồn tại.");

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                bool companyOk = await _context.CompanyEntities
                    .AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken);
                if (!companyOk)
                    throw new InvalidOperationException("Công ty không tồn tại.");
            }
        }
    }

    public class UpdateSalaryConfigCommand : SalaryConfigCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateSalaryConfigCommandHandler : IRequestHandler<UpdateSalaryConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly CreateSalaryConfigCommandHandler _createHandler;

        public UpdateSalaryConfigCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
            _createHandler = new CreateSalaryConfigCommandHandler(context, actionLog);
        }

        public async Task<bool> Handle(UpdateSalaryConfigCommand request, CancellationToken cancellationToken)
        {
            SalaryConfigEntity? entity = await _context.SalaryConfigEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            await _createHandler.ValidateAsync(request, request.Id, cancellationToken);
            object oldValue = SalaryConfigMapper.ToLogObject(entity);
            SalaryConfigMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "SalaryConfigEntity",
                entity.Id,
                oldValue,
                SalaryConfigMapper.ToLogObject(entity),
                "Cập nhật cấu hình lương " + entity.Name);
            return true;
        }
    }

    public class ActivateSalaryConfigCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class ActivateSalaryConfigCommandHandler : IRequestHandler<ActivateSalaryConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivateSalaryConfigCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateSalaryConfigCommand request, CancellationToken cancellationToken)
        {
            SalaryConfigEntity? entity = await _context.SalaryConfigEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            object oldValue = SalaryConfigMapper.ToLogObject(entity);
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "SalaryConfigEntity",
                entity.Id,
                oldValue,
                SalaryConfigMapper.ToLogObject(entity),
                (request.IsActive ? "Kích hoạt " : "Vô hiệu ") + entity.Name);
            return true;
        }
    }
}
