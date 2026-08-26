using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.TransferEmployee;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.EmployeeMovement;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.TransferEmployees.Commands
{
    public class CreateTransferEmployeeCommand : TransferEmployeeCommandFields, IRequest<Guid> { }

    public class CreateTransferEmployeeCommandHandler : IRequestHandler<CreateTransferEmployeeCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly IWorkflowEngine _workflow;

        public CreateTransferEmployeeCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            IWorkflowEngine workflow)
        {
            _context = context;
            _actionLog = actionLog;
            _workflow = workflow;
        }

        public async Task<Guid> Handle(CreateTransferEmployeeCommand request, CancellationToken cancellationToken)
        {
            if (!request.EmployeeId.HasValue || request.EmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã điều chuyển là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(request.TransferType))
            {
                throw new InvalidOperationException("Loại điều chuyển là bắt buộc.");
            }
            if (!request.EffectiveDate.HasValue)
            {
                throw new InvalidOperationException("Ngày hiệu lực là bắt buộc.");
            }
            if (request.Details == null || request.Details.Count == 0)
            {
                throw new InvalidOperationException("Cần ít nhất một dòng chi tiết thay đổi tổ chức.");
            }

            EmployeeEntity employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Nhân viên không tồn tại.");

            await EnsureUniqueCodeAsync(request.Code!, null, cancellationToken);
            NormalizeDetailGuids(request.Details);
            await ValidateOrgFksAsync(request.Details, cancellationToken);

            TransferEmployeeEntity entity = new()
            {
                EmployeeId = request.EmployeeId.Value,
                Code = request.Code.Trim(),
                TransferType = request.TransferType.Trim(),
                RequestDate = request.RequestDate ?? DateTime.UtcNow,
                EffectiveDate = request.EffectiveDate.Value,
                ExpectedEndDate = request.ExpectedEndDate,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                DecisionNumber = string.IsNullOrWhiteSpace(request.DecisionNumber) ? null : request.DecisionNumber.Trim(),
                DecisionDate = request.DecisionDate,
                DecisionFileUrl = string.IsNullOrWhiteSpace(request.DecisionFileUrl) ? null : request.DecisionFileUrl.Trim(),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                Status = TransferStatus.Pending,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            foreach (TransferEmployeePositionInputDto detail in request.Details)
            {
                entity.TransferDetails.Add(BuildDetail(entity, employee, detail, request.EffectiveDate.Value));
            }

            _ = _context.TransferEmployeeEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "TransferEmployeeEntity",
                entity.Id,
                null,
                TransferEmployeeMapper.ToLogObject(entity),
                "Tạo đơn điều chuyển " + entity.Code);

            _ = await _workflow.StartAsync(
                Domain.Enums.WorkflowEntityType.Transfer, entity.Id, employee.CompanyId, cancellationToken);

            return entity.Id;
        }

        internal async Task EnsureUniqueCodeAsync(string code, Guid? excludeId, CancellationToken cancellationToken)
        {
            bool exists = await _context.TransferEmployeeEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã điều chuyển đã tồn tại.");
            }
        }

        internal static void NormalizeDetailGuids(List<TransferEmployeePositionInputDto> details)
        {
            foreach (TransferEmployeePositionInputDto d in details)
            {
                d.OldCompanyId = NullIfEmpty(d.OldCompanyId);
                d.NewCompanyId = NullIfEmpty(d.NewCompanyId);
                d.OldBranchId = NullIfEmpty(d.OldBranchId);
                d.NewBranchId = NullIfEmpty(d.NewBranchId);
                d.OldDepartmentId = NullIfEmpty(d.OldDepartmentId);
                d.NewDepartmentId = NullIfEmpty(d.NewDepartmentId);
                d.OldPartId = NullIfEmpty(d.OldPartId);
                d.NewPartId = NullIfEmpty(d.NewPartId);
                d.OldPositionId = NullIfEmpty(d.OldPositionId);
                d.NewPositionId = NullIfEmpty(d.NewPositionId);
            }
        }

        private static Guid? NullIfEmpty(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty ? value : null;
        }

        internal async Task ValidateOrgFksAsync(List<TransferEmployeePositionInputDto> details, CancellationToken cancellationToken)
        {
            foreach (TransferEmployeePositionInputDto d in details)
            {
                if (d.OldCompanyId.HasValue && d.OldCompanyId != Guid.Empty
                    && !await _context.CompanyEntities.AnyAsync(x => x.Id == d.OldCompanyId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Công ty cũ không tồn tại.");
                }
                if (d.NewCompanyId.HasValue && d.NewCompanyId != Guid.Empty
                    && !await _context.CompanyEntities.AnyAsync(x => x.Id == d.NewCompanyId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Công ty mới không tồn tại.");
                }
                if (d.OldBranchId.HasValue && d.OldBranchId != Guid.Empty
                    && !await _context.BranchEntities.AnyAsync(x => x.Id == d.OldBranchId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Chi nhánh cũ không tồn tại.");
                }
                if (d.NewBranchId.HasValue && d.NewBranchId != Guid.Empty
                    && !await _context.BranchEntities.AnyAsync(x => x.Id == d.NewBranchId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Chi nhánh mới không tồn tại.");
                }
                if (d.OldDepartmentId.HasValue && d.OldDepartmentId != Guid.Empty
                    && !await _context.DepartmentEntities.AnyAsync(x => x.Id == d.OldDepartmentId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Phòng ban cũ không tồn tại.");
                }
                if (d.NewDepartmentId.HasValue && d.NewDepartmentId != Guid.Empty
                    && !await _context.DepartmentEntities.AnyAsync(x => x.Id == d.NewDepartmentId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Phòng ban mới không tồn tại.");
                }
                if (d.OldPartId.HasValue && d.OldPartId != Guid.Empty
                    && !await _context.PartEntities.AnyAsync(x => x.Id == d.OldPartId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Bộ phận cũ không tồn tại.");
                }
                if (d.NewPartId.HasValue && d.NewPartId != Guid.Empty
                    && !await _context.PartEntities.AnyAsync(x => x.Id == d.NewPartId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Bộ phận mới không tồn tại.");
                }
                if (d.OldPositionId.HasValue && d.OldPositionId != Guid.Empty
                    && !await _context.PositionEntities.AnyAsync(x => x.Id == d.OldPositionId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Chức vụ cũ không tồn tại.");
                }
                if (d.NewPositionId.HasValue && d.NewPositionId != Guid.Empty
                    && !await _context.PositionEntities.AnyAsync(x => x.Id == d.NewPositionId && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Chức vụ mới không tồn tại.");
                }

                bool hasChange =
                    (d.NewCompanyId.HasValue && d.NewCompanyId != d.OldCompanyId)
                    || (d.NewBranchId.HasValue && d.NewBranchId != d.OldBranchId)
                    || (d.NewDepartmentId.HasValue && d.NewDepartmentId != d.OldDepartmentId)
                    || (d.NewPartId.HasValue && d.NewPartId != d.OldPartId)
                    || (d.NewPositionId.HasValue && d.NewPositionId != d.OldPositionId)
                    || d.NewCompanyId.HasValue
                    || d.NewBranchId.HasValue
                    || d.NewDepartmentId.HasValue
                    || d.NewPartId.HasValue
                    || d.NewPositionId.HasValue;
                if (!d.NewCompanyId.HasValue
                    && !d.NewBranchId.HasValue
                    && !d.NewDepartmentId.HasValue
                    && !d.NewPartId.HasValue
                    && !d.NewPositionId.HasValue)
                {
                    throw new InvalidOperationException("Mỗi dòng chi tiết cần ít nhất một giá trị tổ chức mới.");
                }
            }
        }

        internal static TransferEmployeePositionEntity BuildDetail(
            TransferEmployeeEntity header,
            EmployeeEntity employee,
            TransferEmployeePositionInputDto detail,
            DateTime effectiveDate)
        {
            Guid? oldCompany = NullIfEmpty(detail.OldCompanyId) ?? employee.CompanyId;
            Guid? oldBranch = NullIfEmpty(detail.OldBranchId) ?? employee.BranchId;
            Guid? oldDepartment = NullIfEmpty(detail.OldDepartmentId) ?? employee.DepartmentId;
            Guid? oldPart = NullIfEmpty(detail.OldPartId) ?? employee.PartId;
            Guid? oldPosition = NullIfEmpty(detail.OldPositionId) ?? employee.PositionId;

            Guid? newCompany = NullIfEmpty(detail.NewCompanyId) ?? oldCompany;
            Guid? newBranch = NullIfEmpty(detail.NewBranchId) ?? oldBranch;
            Guid? newDepartment = NullIfEmpty(detail.NewDepartmentId) ?? oldDepartment;
            Guid? newPart = NullIfEmpty(detail.NewPartId) ?? oldPart;
            Guid? newPosition = NullIfEmpty(detail.NewPositionId) ?? oldPosition;

            bool hasActualChange =
                newCompany != oldCompany
                || newBranch != oldBranch
                || newDepartment != oldDepartment
                || newPart != oldPart
                || newPosition != oldPosition;
            if (!hasActualChange)
            {
                throw new InvalidOperationException("Cần ít nhất một thay đổi tổ chức (giá trị Mới khác Trước).");
            }

            string? changeType = string.IsNullOrWhiteSpace(detail.ChangeType)
                ? ResolveChangeType(oldCompany, newCompany, oldBranch, newBranch, oldDepartment, newDepartment, oldPart, newPart, oldPosition, newPosition)
                : detail.ChangeType.Trim();

            return new TransferEmployeePositionEntity
            {
                EmployeeId = employee.Id,
                EffectiveDate = detail.EffectiveDate ?? effectiveDate,
                OldCompanyId = oldCompany,
                NewCompanyId = newCompany,
                OldBranchId = oldBranch,
                NewBranchId = newBranch,
                OldDepartmentId = oldDepartment,
                NewDepartmentId = newDepartment,
                OldPartId = oldPart,
                NewPartId = newPart,
                OldPositionId = oldPosition,
                NewPositionId = newPosition,
                ChangeType = changeType,
                Note = string.IsNullOrWhiteSpace(detail.Note) ? null : detail.Note.Trim(),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };
        }

        private static string ResolveChangeType(
            Guid? oldCompany, Guid? newCompany,
            Guid? oldBranch, Guid? newBranch,
            Guid? oldDepartment, Guid? newDepartment,
            Guid? oldPart, Guid? newPart,
            Guid? oldPosition, Guid? newPosition)
        {
            int changes = 0;
            string? single = null;
            void Check(Guid? o, Guid? n, string type)
            {
                if (o != n)
                {
                    changes++;
                    single = type;
                }
            }

            Check(oldCompany, newCompany, TransferChangeType.Company);
            Check(oldBranch, newBranch, TransferChangeType.Branch);
            Check(oldDepartment, newDepartment, TransferChangeType.Department);
            Check(oldPart, newPart, TransferChangeType.Part);
            Check(oldPosition, newPosition, TransferChangeType.Position);

            return changes > 1 ? TransferChangeType.Mixed : (single ?? TransferChangeType.Mixed);
        }
    }

    public class UpdateTransferEmployeeCommand : TransferEmployeeCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateTransferEmployeeCommandHandler : IRequestHandler<UpdateTransferEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly CreateTransferEmployeeCommandHandler _createHandler;

        public UpdateTransferEmployeeCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            IWorkflowEngine workflow)
        {
            _context = context;
            _actionLog = actionLog;
            _createHandler = new CreateTransferEmployeeCommandHandler(context, actionLog, workflow);
        }

        public async Task<bool> Handle(UpdateTransferEmployeeCommand request, CancellationToken cancellationToken)
        {
            TransferEmployeeEntity? entity = await _context.TransferEmployeeEntities
                .Include(x => x.TransferDetails)
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != TransferStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ cập nhật đơn đang chờ duyệt.");
            }

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                await _createHandler.EnsureUniqueCodeAsync(request.Code, request.Id, cancellationToken);
            }

            if (request.Details != null && request.Details.Count > 0)
            {
                CreateTransferEmployeeCommandHandler.NormalizeDetailGuids(request.Details);
                await _createHandler.ValidateOrgFksAsync(request.Details, cancellationToken);
            }

            EmployeeEntity employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == entity.EmployeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Nhân viên không tồn tại.");

            object oldValue = TransferEmployeeMapper.ToLogObject(entity);
            TransferEmployeeMapper.ApplyHeaderFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;

            if (request.Details != null && request.Details.Count > 0)
            {
                List<TransferEmployeePositionEntity> existing = entity.TransferDetails.Where(x => !x.IsDeleted).ToList();
                foreach (TransferEmployeePositionEntity d in existing)
                {
                    d.IsDeleted = true;
                    d.UpdatedAt = DateTime.UtcNow;
                }

                DateTime effective = request.EffectiveDate ?? entity.EffectiveDate;
                foreach (TransferEmployeePositionInputDto detail in request.Details)
                {
                    TransferEmployeePositionEntity row = CreateTransferEmployeeCommandHandler.BuildDetail(entity, employee, detail, effective);
                    row.TransferEmployeeId = entity.Id;
                    _ = _context.TransferEmployeePositionEntities.Add(row);
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "TransferEmployeeEntity",
                entity.Id,
                oldValue,
                TransferEmployeeMapper.ToLogObject(entity),
                "Cập nhật đơn điều chuyển " + entity.Code);

            return true;
        }
    }

    public class ApproveTransferEmployeeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
    }

    public class ApproveTransferEmployeeCommandHandler : IRequestHandler<ApproveTransferEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public ApproveTransferEmployeeCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(ApproveTransferEmployeeCommand request, CancellationToken cancellationToken)
        {
            TransferEmployeeEntity? entity = await _context.TransferEmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != TransferStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ duyệt đơn đang chờ duyệt.");
            }

            object oldValue = TransferEmployeeMapper.ToLogObject(entity);
            entity.Status = TransferStatus.Approved;
            entity.ApprovedBy = _currentUser.Username ?? _currentUser.UserCode ?? "System";
            entity.ApprovedDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                entity.Note = request.Note.Trim();
            }
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.APPROVE,
                "TransferEmployeeEntity",
                entity.Id,
                oldValue,
                TransferEmployeeMapper.ToLogObject(entity),
                "Duyệt đơn điều chuyển " + entity.Code);

            return true;
        }
    }

    public class RejectTransferEmployeeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
    }

    public class RejectTransferEmployeeCommandHandler : IRequestHandler<RejectTransferEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public RejectTransferEmployeeCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(RejectTransferEmployeeCommand request, CancellationToken cancellationToken)
        {
            TransferEmployeeEntity? entity = await _context.TransferEmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != TransferStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ từ chối đơn đang chờ duyệt.");
            }

            object oldValue = TransferEmployeeMapper.ToLogObject(entity);
            entity.Status = TransferStatus.Rejected;
            entity.ApprovedBy = _currentUser.Username ?? _currentUser.UserCode ?? "System";
            entity.ApprovedDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                entity.Note = request.Note.Trim();
            }
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.REJECT,
                "TransferEmployeeEntity",
                entity.Id,
                oldValue,
                TransferEmployeeMapper.ToLogObject(entity),
                "Từ chối đơn điều chuyển " + entity.Code);

            return true;
        }
    }

    public class ApplyTransferEmployeeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool Force { get; set; }
    }

    public class ApplyTransferEmployeeCommandHandler : IRequestHandler<ApplyTransferEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ApplyTransferEmployeeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ApplyTransferEmployeeCommand request, CancellationToken cancellationToken)
        {
            TransferEmployeeEntity? entity = await _context.TransferEmployeeEntities
                .Include(x => x.TransferDetails.Where(d => !d.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != TransferStatus.Approved)
            {
                throw new InvalidOperationException("Chỉ áp dụng đơn đã được duyệt.");
            }

            if (!request.Force && entity.EffectiveDate.Date > DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Chưa đến ngày hiệu lực. Bật Force nếu muốn áp dụng sớm.");
            }

            TransferEmployeePositionEntity detail = entity.TransferDetails.FirstOrDefault(d => !d.IsDeleted)
                ?? throw new InvalidOperationException("Đơn điều chuyển không có chi tiết thay đổi.");

            EmployeeEntity employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == entity.EmployeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Nhân viên không tồn tại.");

            object oldValue = TransferEmployeeMapper.ToLogObject(entity);

            foreach (TransferEmployeePositionEntity d in entity.TransferDetails.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedAt))
            {
                if (d.NewCompanyId.HasValue)
                {
                    employee.CompanyId = d.NewCompanyId;
                }

                if (d.NewBranchId.HasValue)
                {
                    employee.BranchId = d.NewBranchId;
                }

                if (d.NewDepartmentId.HasValue)
                {
                    employee.DepartmentId = d.NewDepartmentId;
                }

                if (d.NewPartId.HasValue)
                {
                    employee.PartId = d.NewPartId;
                }

                if (d.NewPositionId.HasValue)
                {
                    employee.PositionId = d.NewPositionId;
                }
            }
            employee.UpdatedAt = DateTime.UtcNow;

            entity.Status = TransferStatus.Applied;
            entity.ActualEndDate = entity.TransferType == TransferType.Secondment ? null : DateTime.UtcNow.Date;
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "TransferEmployeeEntity",
                entity.Id,
                oldValue,
                TransferEmployeeMapper.ToLogObject(entity),
                "Áp dụng đơn điều chuyển " + entity.Code);

            return true;
        }
    }

    public class CancelTransferEmployeeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
    }

    public class CancelTransferEmployeeCommandHandler : IRequestHandler<CancelTransferEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CancelTransferEmployeeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(CancelTransferEmployeeCommand request, CancellationToken cancellationToken)
        {
            TransferEmployeeEntity? entity = await _context.TransferEmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is TransferStatus.Applied or TransferStatus.Rejected or TransferStatus.Cancelled)
            {
                throw new InvalidOperationException("Không thể hủy đơn ở trạng thái hiện tại.");
            }

            object oldValue = TransferEmployeeMapper.ToLogObject(entity);
            entity.Status = TransferStatus.Cancelled;
            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                entity.Note = request.Note.Trim();
            }
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.CANCEL,
                "TransferEmployeeEntity",
                entity.Id,
                oldValue,
                TransferEmployeeMapper.ToLogObject(entity),
                "Hủy đơn điều chuyển " + entity.Code);

            return true;
        }
    }

    public class BulkCreateTransferEmployeeCommand : IRequest<List<Guid>>
    {
        public List<Guid> EmployeeIds { get; set; } = [];
        public string? CodePrefix { get; set; }
        public string? TransferType { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public string? Reason { get; set; }
        public string? DecisionNumber { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? DecisionFileUrl { get; set; }
        public string? Note { get; set; }
        public Guid? NewCompanyId { get; set; }
        public Guid? NewBranchId { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public Guid? NewPartId { get; set; }
        public Guid? NewPositionId { get; set; }
    }

    public class BulkCreateTransferEmployeeCommandHandler : IRequestHandler<BulkCreateTransferEmployeeCommand, List<Guid>>
    {
        private readonly IMediator _mediator;
        private readonly IApplicationDbContext _context;

        public BulkCreateTransferEmployeeCommandHandler(IMediator mediator, IApplicationDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        public async Task<List<Guid>> Handle(BulkCreateTransferEmployeeCommand request, CancellationToken cancellationToken)
        {
            List<Guid> ids = request.EmployeeIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
            {
                throw new InvalidOperationException("Cần chọn ít nhất một nhân viên.");
            }

            if (!request.EffectiveDate.HasValue)
            {
                throw new InvalidOperationException("Ngày hiệu lực là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.TransferType))
            {
                throw new InvalidOperationException("Loại điều chuyển là bắt buộc.");
            }

            if (!request.NewCompanyId.HasValue
                && !request.NewBranchId.HasValue
                && !request.NewDepartmentId.HasValue
                && !request.NewPartId.HasValue
                && !request.NewPositionId.HasValue)
            {
                throw new InvalidOperationException("Cần ít nhất một giá trị tổ chức mới (công ty/CN/PB/bộ phận/chức vụ).");
            }

            List<EmployeeEntity> employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync(cancellationToken);
            if (employees.Count == 0)
            {
                throw new InvalidOperationException("Không tìm thấy nhân viên hợp lệ.");
            }

            var prefix = string.IsNullOrWhiteSpace(request.CodePrefix)
                ? $"DC-{DateTime.UtcNow:yyyyMMdd}"
                : request.CodePrefix.Trim();
            List<Guid> created = new();
            var seq = 1;

            foreach (EmployeeEntity? employee in employees)
            {
                var code = $"{prefix}-{seq:D3}";
                seq++;

                TransferEmployeePositionInputDto detail = new()
                {
                    OldCompanyId = employee.CompanyId,
                    OldBranchId = employee.BranchId,
                    OldDepartmentId = employee.DepartmentId,
                    OldPartId = employee.PartId,
                    OldPositionId = employee.PositionId,
                    NewCompanyId = request.NewCompanyId ?? employee.CompanyId,
                    NewBranchId = request.NewBranchId,
                    NewDepartmentId = request.NewDepartmentId,
                    NewPartId = request.NewPartId,
                    NewPositionId = request.NewPositionId,
                    EffectiveDate = request.EffectiveDate,
                    ChangeType = TransferChangeType.Mixed,
                };

                if (!request.NewBranchId.HasValue)
                {
                    detail.NewBranchId = employee.BranchId;
                }

                if (!request.NewDepartmentId.HasValue)
                {
                    detail.NewDepartmentId = employee.DepartmentId;
                }

                if (!request.NewPartId.HasValue)
                {
                    detail.NewPartId = employee.PartId;
                }

                if (!request.NewPositionId.HasValue)
                {
                    detail.NewPositionId = employee.PositionId;
                }

                Guid id = await _mediator.Send(new CreateTransferEmployeeCommand
                {
                    EmployeeId = employee.Id,
                    Code = code,
                    TransferType = request.TransferType,
                    RequestDate = request.RequestDate ?? DateTime.UtcNow,
                    EffectiveDate = request.EffectiveDate,
                    ExpectedEndDate = request.ExpectedEndDate,
                    Reason = request.Reason,
                    DecisionNumber = request.DecisionNumber,
                    DecisionDate = request.DecisionDate,
                    DecisionFileUrl = request.DecisionFileUrl,
                    Note = request.Note,
                    Details = [detail],
                }, cancellationToken);

                created.Add(id);
            }

            return created;
        }
    }
}
