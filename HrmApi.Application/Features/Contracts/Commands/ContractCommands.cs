using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Contracts.Commands
{
    public class CreateContractCommand : ContractCommandFields, IRequest<Guid> { }

    public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateContractCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateContractCommand request, CancellationToken cancellationToken)
        {
            if (!request.EmployeeId.HasValue || request.EmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Số hợp đồng là bắt buộc.");
            }
            if (!request.StartDate.HasValue)
            {
                throw new InvalidOperationException("Ngày bắt đầu hợp đồng là bắt buộc.");
            }

            EmployeeEntity employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Nhân viên không tồn tại.");

            ContractTypeEntity? contractType = null;
            if (request.ContractTypeId.HasValue && request.ContractTypeId != Guid.Empty)
            {
                contractType = await _context.ContractTypeEntities
                    .FirstOrDefaultAsync(x => x.Id == request.ContractTypeId && !x.IsDeleted && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException("Loại hợp đồng không tồn tại hoặc đã bị vô hiệu.");
            }

            await EnsureUniqueCodeAsync(request.Code!, null, cancellationToken);

            bool hasActive = await _context.ContractEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.EmployeeId == request.EmployeeId
                && (x.Status == ContractStatus.Active || x.Status == ContractStatus.ExpiringSoon),
                cancellationToken);
            if (hasActive && (request.Status == ContractStatus.Active || request.Status == ContractStatus.ExpiringSoon || string.IsNullOrWhiteSpace(request.Status)))
            {
                // Cho phép tạo nháp/chờ ký song song; chỉ chặn khi cố tạo ACTIVE khi đã có HĐ hiệu lực
            }

            DateTime? endDate = request.EndDate;
            if (contractType != null)
            {
                if (contractType.IsUnlimited)
                {
                    endDate = null;
                }
                else if (!endDate.HasValue && contractType.DefaultDurationMonths.HasValue && contractType.DefaultDurationMonths > 0)
                {
                    endDate = request.StartDate.Value.AddMonths(contractType.DefaultDurationMonths.Value);
                }
            }

            string status = string.IsNullOrWhiteSpace(request.Status) ? ContractStatus.Draft : request.Status.Trim();
            if (status == ContractStatus.Active)
            {
                bool conflict = await _context.ContractEntities.AnyAsync(x =>
                    !x.IsDeleted
                    && x.EmployeeId == request.EmployeeId
                    && (x.Status == ContractStatus.Active || x.Status == ContractStatus.ExpiringSoon),
                    cancellationToken);
                if (conflict)
                {
                    throw new InvalidOperationException("Nhân viên đã có hợp đồng đang hiệu lực. Vui lòng chấm dứt hoặc hết hạn hợp đồng cũ trước.");
                }
            }

            ContractEntity entity = new()
            {
                EmployeeId = request.EmployeeId.Value,
                ContractTypeId = request.ContractTypeId,
                Code = request.Code.Trim(),
                StartDate = request.StartDate.Value,
                EndDate = endDate,
                CompanyId = employee.CompanyId,
                BranchId = employee.BranchId,
                DepartmentId = employee.DepartmentId,
                PositionId = employee.PositionId,
                JobTitle = string.IsNullOrWhiteSpace(request.JobTitle) ? null : request.JobTitle.Trim(),
                WorkingLocation = string.IsNullOrWhiteSpace(request.WorkingLocation) ? null : request.WorkingLocation.Trim(),
                BasicSalary = request.BasicSalary,
                Allowance = request.Allowance,
                InsuranceSalary = request.InsuranceSalary,
                PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? null : request.PaymentMethod.Trim(),
                SignedByCompanyRepresentative = string.IsNullOrWhiteSpace(request.SignedByCompanyRepresentative)
                    ? null
                    : request.SignedByCompanyRepresentative.Trim(),
                SignedByEmployeeName = string.IsNullOrWhiteSpace(request.SignedByEmployeeName)
                    ? employee.FullName
                    : request.SignedByEmployeeName.Trim(),
                IsAutoRenew = request.IsAutoRenew ?? false,
                FileUrl = string.IsNullOrWhiteSpace(request.FileUrl) ? null : request.FileUrl.Trim(),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                SignDate = request.SignDate,
                Status = status,
                RenewalTimes = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            _ = _context.ContractEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "ContractEntity",
                entity.Id,
                null,
                ContractMapper.ToLogObject(entity),
                "Tạo hợp đồng " + entity.Code);

            return entity.Id;
        }

        internal async Task EnsureUniqueCodeAsync(string code, Guid? excludeId, CancellationToken cancellationToken)
        {
            bool exists = await _context.ContractEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Số hợp đồng đã tồn tại.");
            }
        }
    }

    public class UpdateContractCommand : ContractCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateContractCommandHandler : IRequestHandler<UpdateContractCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly CreateContractCommandHandler _createHandler;

        public UpdateContractCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
            _createHandler = new CreateContractCommandHandler(context, actionLog);
        }

        public async Task<bool> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
        {
            ContractEntity? entity = await _context.ContractEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is ContractStatus.Terminated or ContractStatus.Liquidated or ContractStatus.Expired)
            {
                throw new InvalidOperationException("Không thể cập nhật hợp đồng đã kết thúc.");
            }

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                await _createHandler.EnsureUniqueCodeAsync(request.Code, request.Id, cancellationToken);
            }

            if (request.ContractTypeId.HasValue && request.ContractTypeId != Guid.Empty)
            {
                bool typeOk = await _context.ContractTypeEntities
                    .AnyAsync(x => x.Id == request.ContractTypeId && !x.IsDeleted, cancellationToken);
                if (!typeOk)
                {
                    throw new InvalidOperationException("Loại hợp đồng không tồn tại.");
                }
            }

            object oldValue = ContractMapper.ToLogObject(entity);
            ContractMapper.ApplyCommandFields(entity, request);

            if (request.ContractTypeId.HasValue)
            {
                ContractTypeEntity? type = await _context.ContractTypeEntities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ContractTypeId, cancellationToken);
                if (type?.IsUnlimited == true)
                {
                    entity.EndDate = null;
                }
                else if (!request.EndDate.HasValue && type?.DefaultDurationMonths is > 0 && request.StartDate.HasValue)
                {
                    entity.EndDate = request.StartDate.Value.AddMonths(type.DefaultDurationMonths.Value);
                }
            }

            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "ContractEntity",
                entity.Id,
                oldValue,
                ContractMapper.ToLogObject(entity),
                "Cập nhật hợp đồng " + entity.Code);

            return true;
        }
    }

    public class SignContractCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public DateTime? SignDate { get; set; }
        public string? SignedByCompanyRepresentative { get; set; }
        public string? SignedByEmployeeName { get; set; }
        public string? FileUrl { get; set; }
    }

    public class SignContractCommandHandler : IRequestHandler<SignContractCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public SignContractCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(SignContractCommand request, CancellationToken cancellationToken)
        {
            ContractEntity? entity = await _context.ContractEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is not (ContractStatus.Draft or ContractStatus.PendingSign))
            {
                throw new InvalidOperationException("Chỉ ký hợp đồng đang ở trạng thái Nháp hoặc Chờ ký.");
            }

            bool conflict = await _context.ContractEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Id != entity.Id
                && x.EmployeeId == entity.EmployeeId
                && (x.Status == ContractStatus.Active || x.Status == ContractStatus.ExpiringSoon),
                cancellationToken);
            if (conflict)
            {
                throw new InvalidOperationException("Nhân viên đã có hợp đồng đang hiệu lực.");
            }

            object oldValue = ContractMapper.ToLogObject(entity);
            entity.SignDate = request.SignDate ?? DateTime.UtcNow.Date;
            if (!string.IsNullOrWhiteSpace(request.SignedByCompanyRepresentative))
            {
                entity.SignedByCompanyRepresentative = request.SignedByCompanyRepresentative.Trim();
            }
            if (!string.IsNullOrWhiteSpace(request.SignedByEmployeeName))
            {
                entity.SignedByEmployeeName = request.SignedByEmployeeName.Trim();
            }
            else if (string.IsNullOrWhiteSpace(entity.SignedByEmployeeName))
            {
                entity.SignedByEmployeeName = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == entity.EmployeeId)
                    .Select(x => x.FullName)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            if (!string.IsNullOrWhiteSpace(request.FileUrl))
            {
                entity.FileUrl = request.FileUrl.Trim();
            }
            entity.Status = ContractStatus.Active;
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "ContractEntity",
                entity.Id,
                oldValue,
                ContractMapper.ToLogObject(entity),
                "Ký hợp đồng " + entity.Code);

            return true;
        }
    }

    public class TerminateContractCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string? TerminationReason { get; set; }
        public bool SyncEmployeeResignation { get; set; } = true;
    }

    public class TerminateContractCommandHandler : IRequestHandler<TerminateContractCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public TerminateContractCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(TerminateContractCommand request, CancellationToken cancellationToken)
        {
            ContractEntity? entity = await _context.ContractEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is ContractStatus.Terminated or ContractStatus.Liquidated or ContractStatus.Expired)
            {
                throw new InvalidOperationException("Hợp đồng đã kết thúc.");
            }

            if (string.IsNullOrWhiteSpace(request.TerminationReason))
            {
                throw new InvalidOperationException("Lý do chấm dứt hợp đồng là bắt buộc.");
            }

            object oldValue = ContractMapper.ToLogObject(entity);
            entity.TerminationDate = request.TerminationDate ?? DateTime.UtcNow.Date;
            entity.TerminationReason = request.TerminationReason.Trim();
            entity.Status = ContractStatus.Terminated;
            entity.UpdatedAt = DateTime.UtcNow;

            if (request.SyncEmployeeResignation)
            {
                EmployeeEntity? employee = await _context.EmployeeEntities
                    .FirstOrDefaultAsync(x => x.Id == entity.EmployeeId && !x.IsDeleted, cancellationToken);
                if (employee != null)
                {
                    employee.Status = "RESIGNED";
                    employee.ResignationDate = entity.TerminationDate;
                    employee.UpdatedAt = DateTime.UtcNow;
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "ContractEntity",
                entity.Id,
                oldValue,
                ContractMapper.ToLogObject(entity),
                "Chấm dứt hợp đồng " + entity.Code);

            return true;
        }
    }

    public class RenewContractCommand : IRequest<Guid>
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid? ContractTypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? Allowance { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public string? Note { get; set; }
        public bool SignImmediately { get; set; } = true;
    }

    public class RenewContractCommandHandler : IRequestHandler<RenewContractCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly CreateContractCommandHandler _createHandler;

        public RenewContractCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
            _createHandler = new CreateContractCommandHandler(context, actionLog);
        }

        public async Task<Guid> Handle(RenewContractCommand request, CancellationToken cancellationToken)
        {
            ContractEntity oldContract = await _context.ContractEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Hợp đồng không tồn tại.");

            if (oldContract.Status is not (ContractStatus.Active or ContractStatus.ExpiringSoon or ContractStatus.Expired))
            {
                throw new InvalidOperationException("Chỉ gia hạn hợp đồng đang hiệu lực, sắp hết hạn hoặc đã hết hạn.");
            }

            Guid? typeId = request.ContractTypeId ?? oldContract.ContractTypeId;
            ContractTypeEntity? contractType = null;
            if (typeId.HasValue)
            {
                contractType = await _context.ContractTypeEntities
                    .FirstOrDefaultAsync(x => x.Id == typeId && !x.IsDeleted && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException("Loại hợp đồng không tồn tại hoặc đã bị vô hiệu.");

                if (contractType.MaxRenewalTimes.HasValue)
                {
                    int nextRenewal = oldContract.RenewalTimes + 1;
                    if (nextRenewal > contractType.MaxRenewalTimes.Value)
                    {
                        throw new InvalidOperationException(
                            $"Đã vượt số lần gia hạn tối đa ({contractType.MaxRenewalTimes.Value}) của loại hợp đồng này.");
                    }
                }
            }

            DateTime startDate = request.StartDate
                ?? (oldContract.EndDate?.Date.AddDays(1) ?? DateTime.UtcNow.Date);
            DateTime? endDate = request.EndDate;
            if (contractType != null)
            {
                if (contractType.IsUnlimited)
                {
                    endDate = null;
                }
                else if (!endDate.HasValue && contractType.DefaultDurationMonths is > 0)
                {
                    endDate = startDate.AddMonths(contractType.DefaultDurationMonths.Value);
                }
            }

            string code = string.IsNullOrWhiteSpace(request.Code)
                ? $"{oldContract.Code}-R{oldContract.RenewalTimes + 1}"
                : request.Code.Trim();
            await _createHandler.EnsureUniqueCodeAsync(code, null, cancellationToken);

            object oldValue = ContractMapper.ToLogObject(oldContract);
            if (oldContract.Status is ContractStatus.Active or ContractStatus.ExpiringSoon)
            {
                oldContract.Status = ContractStatus.Expired;
                oldContract.UpdatedAt = DateTime.UtcNow;
            }

            ContractEntity newContract = new()
            {
                EmployeeId = oldContract.EmployeeId,
                ContractTypeId = typeId,
                Code = code,
                StartDate = startDate,
                EndDate = endDate,
                JobTitle = oldContract.JobTitle,
                WorkingLocation = oldContract.WorkingLocation,
                BasicSalary = request.BasicSalary ?? oldContract.BasicSalary,
                Allowance = request.Allowance ?? oldContract.Allowance,
                InsuranceSalary = request.InsuranceSalary ?? oldContract.InsuranceSalary,
                PaymentMethod = oldContract.PaymentMethod,
                CompanyId = oldContract.CompanyId,
                BranchId = oldContract.BranchId,
                DepartmentId = oldContract.DepartmentId,
                PositionId = oldContract.PositionId,
                SignedByCompanyRepresentative = oldContract.SignedByCompanyRepresentative,
                SignedByEmployeeName = oldContract.SignedByEmployeeName,
                IsAutoRenew = oldContract.IsAutoRenew,
                PreviousContractId = oldContract.Id,
                RenewalTimes = oldContract.RenewalTimes + 1,
                Status = request.SignImmediately ? ContractStatus.Active : ContractStatus.Draft,
                SignDate = request.SignImmediately ? DateTime.UtcNow.Date : null,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            _ = _context.ContractEntities.Add(newContract);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "ContractEntity",
                oldContract.Id,
                oldValue,
                ContractMapper.ToLogObject(oldContract),
                "Hết hạn hợp đồng cũ khi gia hạn " + oldContract.Code);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "ContractEntity",
                newContract.Id,
                null,
                ContractMapper.ToLogObject(newContract),
                "Gia hạn hợp đồng " + newContract.Code);

            return newContract.Id;
        }
    }
}
