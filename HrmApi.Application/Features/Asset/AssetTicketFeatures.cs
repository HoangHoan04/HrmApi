using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Asset;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Asset;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Asset
{
    public class GetAssetTicketsPagedQuery : AssetTicketPagedQuery, IRequest<PagedResult<AssetTicketDto>> { }

    public class GetAssetTicketsPagedQueryHandler : IRequestHandler<GetAssetTicketsPagedQuery, PagedResult<AssetTicketDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetAssetTicketsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AssetTicketDto>> Handle(GetAssetTicketsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetTicketEntity> query = _context.AssetTicketEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.AssetId.HasValue && request.AssetId != Guid.Empty)
            {
                query = query.Where(x => x.AssetId == request.AssetId);
            }

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId || x.ToEmployeeId == request.EmployeeId);
            }

            if (!string.IsNullOrWhiteSpace(request.TicketType))
            {
                query = query.Where(x => x.TicketType == request.TicketType.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s)
                    || (x.Note != null && x.Note.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);
            List<AssetTicketEntity> rows = await query
                .OrderByDescending(x => x.TicketAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<AssetTicketDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<AssetTicketDto>> MapManyAsync(List<AssetTicketEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> assetIds = rows.Select(x => x.AssetId).Distinct().ToList();
            List<Guid> empIds = rows.Select(x => x.EmployeeId)
                .Concat(rows.Where(x => x.ToEmployeeId.HasValue).Select(x => x.ToEmployeeId!.Value))
                .Distinct().ToList();
            List<Guid> companyIds = rows.Select(x => x.CompanyId).Distinct().ToList();
            List<Guid> branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            var assets = await _context.AssetEntities.AsNoTracking()
                .Where(x => assetIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, x.Name }, cancellationToken);
            var emps = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, Name = x.FullName ?? (x.LastName + " " + x.FirstName).Trim() }, cancellationToken);
            Dictionary<Guid, string> companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> branches = branchIds.Count == 0
                ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e =>
            {
                _ = assets.TryGetValue(e.AssetId, out var asset);
                _ = emps.TryGetValue(e.EmployeeId, out var emp);
                _ = emps.TryGetValue(e.ToEmployeeId ?? Guid.Empty, out var toEmp);

                return new AssetTicketDto
                {
                    Id = e.Id,
                    Code = e.Code,
                    AssetId = e.AssetId,
                    AssetCode = asset?.Code,
                    AssetName = asset?.Name,
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = emp?.Code,
                    EmployeeName = emp?.Name,
                    ToEmployeeId = e.ToEmployeeId,
                    ToEmployeeCode = toEmp?.Code,
                    ToEmployeeName = toEmp?.Name,
                    CompanyId = e.CompanyId,
                    CompanyName = companies.GetValueOrDefault(e.CompanyId),
                    BranchId = e.BranchId,
                    BranchName = e.BranchId.HasValue ? branches.GetValueOrDefault(e.BranchId.Value) : null,
                    TicketType = e.TicketType,
                    Status = e.Status,
                    TicketAt = e.TicketAt,
                    ReturnExpectedDate = e.ReturnExpectedDate,
                    Condition = e.Condition,
                    Note = e.Note,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                };
            }).ToList();
        }
    }

    public class GetAssetTicketByIdQuery : IdRequest, IRequest<AssetTicketDto?> { }

    public class GetAssetTicketByIdQueryHandler : IRequestHandler<GetAssetTicketByIdQuery, AssetTicketDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetAssetTicketsPagedQueryHandler _mapper;
        public GetAssetTicketByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetAssetTicketsPagedQueryHandler(context);
        }

        public async Task<AssetTicketDto?> Handle(GetAssetTicketByIdQuery request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? e = await _context.AssetTicketEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateAssetTicketCommand : AssetTicketCommandFields, IRequest<Guid> { }

    public class CreateAssetTicketCommandHandler : IRequestHandler<CreateAssetTicketCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public CreateAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateAssetTicketCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            string ticketType = string.IsNullOrWhiteSpace(request.TicketType)
                ? AssetTicketType.Issue
                : request.TicketType.Trim().ToUpperInvariant();
            string status = string.IsNullOrWhiteSpace(request.Status)
                ? AssetTicketStatus.Draft
                : request.Status.Trim().ToUpperInvariant();

            AssetTicketEntity entity = new()
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                AssetId = request.AssetId!.Value,
                EmployeeId = request.EmployeeId!.Value,
                ToEmployeeId = request.ToEmployeeId,
                CompanyId = request.CompanyId!.Value,
                BranchId = request.BranchId,
                TicketType = ticketType,
                Status = status,
                TicketAt = request.TicketAt ?? DateTime.UtcNow,
                ReturnExpectedDate = request.ReturnExpectedDate,
                Condition = request.Condition,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.AssetTicketEntities.Add(entity);

            if (status == AssetTicketStatus.Done)
            {
                await ApplyCompleteEffectsAsync(entity, cancellationToken);
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "AssetTicketEntity",
                entity.Id,
                null,
                new { entity.Id, entity.Code, entity.TicketType, entity.Status },
                "Tạo mới phiếu tài sản " + entity.Code);

            return entity.Id;
        }

        internal async Task ValidateAsync(AssetTicketCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã phiếu là bắt buộc.");
            }

            if (!request.AssetId.HasValue || request.AssetId == Guid.Empty)
            {
                throw new InvalidOperationException("Tài sản là bắt buộc.");
            }

            if (!request.EmployeeId.HasValue || request.EmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            }

            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
            {
                throw new InvalidOperationException("Công ty là bắt buộc.");
            }

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.AssetTicketEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
            {
                throw new InvalidOperationException("Mã phiếu đã tồn tại.");
            }

            AssetEntity? asset = await _context.AssetEntities.FirstOrDefaultAsync(x => x.Id == request.AssetId && !x.IsDeleted, cancellationToken);
            if (asset == null)
            {
                throw new InvalidOperationException("Tài sản không tồn tại.");
            }

            if (!await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Nhân viên không tồn tại.");
            }

            if (!await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }

            string ticketType = string.IsNullOrWhiteSpace(request.TicketType) ? AssetTicketType.Issue : request.TicketType.Trim().ToUpperInvariant();

            if (ticketType == AssetTicketType.Issue)
            {
                if (asset.Status != AssetStatus.Available && !excludeId.HasValue)
                {
                    throw new InvalidOperationException($"Tài sản đang ở trạng thái '{asset.Status}' — chỉ tài sản 'AVAILABLE' (Sẵn sàng) mới được cấp phát.");
                }
            }
            else if (ticketType == AssetTicketType.Return)
            {
                if (asset.Status != AssetStatus.Assigned && !excludeId.HasValue)
                {
                    throw new InvalidOperationException($"Tài sản đang ở trạng thái '{asset.Status}' — chỉ tài sản 'ASSIGNED' (Đang sử dụng) mới được thu hồi.");
                }
            }
            else if (ticketType == AssetTicketType.Transfer)
            {
                if (!request.ToEmployeeId.HasValue || request.ToEmployeeId == Guid.Empty)
                {
                    throw new InvalidOperationException("Cần chọn nhân viên tiếp nhận khi điều chuyển tài sản.");
                }

                if (request.ToEmployeeId == request.EmployeeId)
                {
                    throw new InvalidOperationException("Nhân viên tiếp nhận phải khác nhân viên bàn giao.");
                }

                if (!await _context.EmployeeEntities.AnyAsync(x => x.Id == request.ToEmployeeId.Value && !x.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException("Nhân viên tiếp nhận không tồn tại.");
                }
            }
        }

        internal async Task ApplyCompleteEffectsAsync(AssetTicketEntity ticket, CancellationToken cancellationToken)
        {
            AssetEntity? asset = await _context.AssetEntities.FirstOrDefaultAsync(x => x.Id == ticket.AssetId && !x.IsDeleted, cancellationToken);
            if (asset == null)
            {
                return;
            }

            DateTime now = ticket.TicketAt;

            if (ticket.TicketType == AssetTicketType.Issue)
            {
                asset.Status = AssetStatus.Assigned;
                AssetAssignmentEntity assignment = new()
                {
                    AssetId = asset.Id,
                    EmployeeId = ticket.EmployeeId,
                    CompanyId = ticket.CompanyId,
                    BranchId = ticket.BranchId ?? asset.BranchId,
                    IssuedAt = now,
                    IssuedTicketId = ticket.Id,
                    ConditionOnIssue = ticket.Condition,
                    Note = ticket.Note,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                };
                _ = _context.AssetAssignmentEntities.Add(assignment);
            }
            else if (ticket.TicketType == AssetTicketType.Return)
            {
                asset.Status = AssetStatus.Available;
                AssetAssignmentEntity? openAssignment = await _context.AssetAssignmentEntities
                    .Where(x => x.AssetId == asset.Id && !x.ReturnedAt.HasValue && !x.IsDeleted)
                    .OrderByDescending(x => x.IssuedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (openAssignment != null)
                {
                    openAssignment.ReturnedAt = now;
                    openAssignment.ReturnedTicketId = ticket.Id;
                    openAssignment.ConditionOnReturn = ticket.Condition;
                    openAssignment.UpdatedAt = DateTime.UtcNow;
                    openAssignment.UpdatedBy = _currentUser.UserId;
                }
            }
            else if (ticket.TicketType == AssetTicketType.Repair)
            {
                asset.Status = AssetStatus.Maintenance;
            }
            else if (ticket.TicketType == AssetTicketType.Transfer && ticket.ToEmployeeId.HasValue)
            {
                asset.Status = AssetStatus.Assigned;
                AssetAssignmentEntity? openAssignment = await _context.AssetAssignmentEntities
                    .Where(x => x.AssetId == asset.Id && !x.ReturnedAt.HasValue && !x.IsDeleted)
                    .OrderByDescending(x => x.IssuedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (openAssignment != null)
                {
                    openAssignment.ReturnedAt = now;
                    openAssignment.ReturnedTicketId = ticket.Id;
                    openAssignment.ConditionOnReturn = "Điều chuyển sang nhân viên khác";
                    openAssignment.UpdatedAt = DateTime.UtcNow;
                    openAssignment.UpdatedBy = _currentUser.UserId;
                }

                AssetAssignmentEntity newAssignment = new()
                {
                    AssetId = asset.Id,
                    EmployeeId = ticket.ToEmployeeId.Value,
                    CompanyId = ticket.CompanyId,
                    BranchId = ticket.BranchId ?? asset.BranchId,
                    IssuedAt = now,
                    IssuedTicketId = ticket.Id,
                    ConditionOnIssue = ticket.Condition,
                    Note = "Nhận điều chuyển từ phiếu " + ticket.Code,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                };
                _ = _context.AssetAssignmentEntities.Add(newAssignment);
            }

            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = _currentUser.UserId;
        }
    }

    public class UpdateAssetTicketCommand : AssetTicketCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateAssetTicketCommandHandler : IRequestHandler<UpdateAssetTicketCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly CreateAssetTicketCommandHandler _create;

        public UpdateAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _create = new CreateAssetTicketCommandHandler(context, currentUser, actionLog);
        }

        public async Task<bool> Handle(UpdateAssetTicketCommand request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? entity = await _context.AssetTicketEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == AssetTicketStatus.Done)
            {
                throw new InvalidOperationException("Phiếu đã hoàn tất — không thể sửa thông tin.");
            }

            request.Code ??= entity.Code;
            request.AssetId ??= entity.AssetId;
            request.EmployeeId ??= entity.EmployeeId;
            request.CompanyId ??= entity.CompanyId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.AssetId = request.AssetId!.Value;
            entity.EmployeeId = request.EmployeeId!.Value;
            entity.ToEmployeeId = request.ToEmployeeId;
            entity.CompanyId = request.CompanyId!.Value;
            entity.BranchId = request.BranchId;
            if (!string.IsNullOrWhiteSpace(request.TicketType))
            {
                entity.TicketType = request.TicketType.Trim().ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                entity.Status = request.Status.Trim().ToUpperInvariant();
            }

            entity.TicketAt = request.TicketAt ?? entity.TicketAt;
            entity.ReturnExpectedDate = request.ReturnExpectedDate;
            entity.Condition = request.Condition;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            if (entity.Status == AssetTicketStatus.Done)
            {
                await _create.ApplyCompleteEffectsAsync(entity, cancellationToken);
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "AssetTicketEntity",
                entity.Id,
                null,
                new { entity.Id, entity.Code, entity.TicketType, entity.Status },
                "Cập nhật phiếu tài sản " + entity.Code);

            return true;
        }
    }

    public class CompleteAssetTicketCommand : IdRequest, IRequest<bool> { }

    public class CompleteAssetTicketCommandHandler : IRequestHandler<CompleteAssetTicketCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly CreateAssetTicketCommandHandler _create;

        public CompleteAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _create = new CreateAssetTicketCommandHandler(context, currentUser, actionLog);
        }

        public async Task<bool> Handle(CompleteAssetTicketCommand request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? entity = await _context.AssetTicketEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == AssetTicketStatus.Cancelled)
            {
                throw new InvalidOperationException("Phiếu đã hủy — không thể hoàn tất.");
            }

            if (entity.Status == AssetTicketStatus.Done)
            {
                throw new InvalidOperationException("Phiếu đã hoàn tất trước đó.");
            }

            entity.Status = AssetTicketStatus.Done;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            await _create.ApplyCompleteEffectsAsync(entity, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "AssetTicketEntity",
                entity.Id,
                null,
                new { entity.Id, entity.Code, entity.TicketType, entity.Status },
                "Hoàn tất xử lý phiếu tài sản " + entity.Code);

            return true;
        }
    }

    public class CancelAssetTicketCommand : IdRequest, IRequest<bool> { }

    public class CancelAssetTicketCommandHandler : IRequestHandler<CancelAssetTicketCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public CancelAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(CancelAssetTicketCommand request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? entity = await _context.AssetTicketEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == AssetTicketStatus.Cancelled)
            {
                throw new InvalidOperationException("Phiếu đã ở trạng thái hủy.");
            }

            bool wasDone = entity.Status == AssetTicketStatus.Done;
            entity.Status = AssetTicketStatus.Cancelled;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            if (wasDone)
            {
                AssetEntity? asset = await _context.AssetEntities.FirstOrDefaultAsync(x => x.Id == entity.AssetId && !x.IsDeleted, cancellationToken);
                if (asset != null)
                {
                    if (entity.TicketType == AssetTicketType.Issue)
                    {
                        asset.Status = AssetStatus.Available;
                        AssetAssignmentEntity? assignment = await _context.AssetAssignmentEntities
                            .FirstOrDefaultAsync(x => x.IssuedTicketId == entity.Id && !x.IsDeleted, cancellationToken);
                        assignment?.IsDeleted = true;
                    }
                    else if (entity.TicketType == AssetTicketType.Return)
                    {
                        asset.Status = AssetStatus.Assigned;
                        AssetAssignmentEntity? assignment = await _context.AssetAssignmentEntities
                            .FirstOrDefaultAsync(x => x.ReturnedTicketId == entity.Id && !x.IsDeleted, cancellationToken);
                        if (assignment != null)
                        {
                            assignment.ReturnedAt = null;
                            assignment.ReturnedTicketId = null;
                        }
                    }
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "AssetTicketEntity",
                entity.Id,
                null,
                new { entity.Id, entity.Code, entity.Status },
                "Hủy phiếu tài sản " + entity.Code);

            return true;
        }
    }

    public class DeleteAssetTicketCommand : IdRequest, IRequest<bool> { }

    public class DeleteAssetTicketCommandHandler : IRequestHandler<DeleteAssetTicketCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public DeleteAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteAssetTicketCommand request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? entity = await _context.AssetTicketEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == AssetTicketStatus.Done)
            {
                throw new InvalidOperationException("Không thể xóa phiếu đã hoàn tất — vui lòng sử dụng chức năng Hủy phiếu.");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DELETE,
                "AssetTicketEntity",
                entity.Id,
                null,
                null,
                "Xóa phiếu tài sản " + entity.Code);

            return true;
        }
    }
}
