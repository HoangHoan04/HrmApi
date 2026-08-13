using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PayrollAdjustments
{
    #region DTOs
    public class AllowanceDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public decimal? DefaultAmount { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsInsurable { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class AdvanceDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public int? DeductMonth { get; set; }
        public int? DeductYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Note { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class PayrollSlipDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public decimal Amount { get; set; }
        public DateTime SlipDate { get; set; }
        public string? SlipType { get; set; }
        public int? ApplyMonth { get; set; }
        public int? ApplyYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Note { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Kind { get; set; } = SlipKind.Deduction;
    }
    #endregion

    #region Allowance
    public class GetAllowancesPagedQuery : PagedRequest, IRequest<PagedResult<AllowanceDto>>
    {
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAllowancesPagedQueryHandler : IRequestHandler<GetAllowancesPagedQuery, PagedResult<AllowanceDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetAllowancesPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<AllowanceDto>> Handle(GetAllowancesPagedQuery request, CancellationToken ct)
        {
            var q = _context.AllowanceEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                q = q.Where(x => x.CompanyId == request.CompanyId || x.CompanyId == null);
            if (request.IsActive.HasValue)
                q = q.Where(x => x.IsActive == request.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                q = q.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
            }

            int total = await q.CountAsync(ct);
            var rows = await q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code)
                .Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);

            var companyIds = rows.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var companies = companyIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.CompanyEntities.AsNoTracking().Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

            var items = rows.Select(x => new AllowanceDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                CompanyId = x.CompanyId,
                CompanyName = x.CompanyId.HasValue && companies.TryGetValue(x.CompanyId.Value, out var n) ? n : null,
                DefaultAmount = x.DefaultAmount,
                IsTaxable = x.IsTaxable,
                IsInsurable = x.IsInsurable,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
            }).ToList();

            return new PagedResult<AllowanceDto>(items, total, request.PageIndex, request.PageSize);
        }
    }

    public class UpsertAllowanceCommand : IRequest<Guid>
    {
        public Guid? Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public decimal? DefaultAmount { get; set; }
        public bool IsTaxable { get; set; } = true;
        public bool IsInsurable { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class UpsertAllowanceCommandHandler : IRequestHandler<UpsertAllowanceCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpsertAllowanceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(UpsertAllowanceCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Mã và tên phụ cấp bắt buộc.");

            string code = request.Code.Trim().ToUpperInvariant();
            AllowanceEntity? entity = null;
            if (request.Id.HasValue && request.Id != Guid.Empty)
            {
                entity = await _context.AllowanceEntities.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
                    ?? throw new InvalidOperationException("Không tìm thấy phụ cấp.");
            }

            bool dup = await _context.AllowanceEntities.AnyAsync(x =>
                !x.IsDeleted && x.Code == code && x.CompanyId == request.CompanyId
                && (!request.Id.HasValue || x.Id != request.Id), ct);
            if (dup) throw new InvalidOperationException("Mã phụ cấp đã tồn tại.");

            if (entity == null)
            {
                entity = new AllowanceEntity
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                };
                _context.AllowanceEntities.Add(entity);
            }

            entity.Code = code;
            entity.Name = request.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.CompanyId = request.CompanyId;
            entity.DefaultAmount = request.DefaultAmount;
            entity.IsTaxable = request.IsTaxable;
            entity.IsInsurable = request.IsInsurable;
            entity.IsActive = request.IsActive;
            entity.DisplayOrder = request.DisplayOrder;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }

    public class SetAllowanceActiveCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class SetAllowanceActiveCommandHandler : IRequestHandler<SetAllowanceActiveCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public SetAllowanceActiveCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(SetAllowanceActiveCommand request, CancellationToken ct)
        {
            var entity = await _context.AllowanceEntities.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct);
            if (entity == null) return false;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
    #endregion

    #region Advance
    public class GetAdvancesPagedQuery : PagedRequest, IRequest<PagedResult<AdvanceDto>>
    {
        public Guid? EmployeeId { get; set; }
        public string? Status { get; set; }
        public int? DeductYear { get; set; }
        public int? DeductMonth { get; set; }
    }

    public class GetAdvancesPagedQueryHandler : IRequestHandler<GetAdvancesPagedQuery, PagedResult<AdvanceDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetAdvancesPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<AdvanceDto>> Handle(GetAdvancesPagedQuery request, CancellationToken ct)
        {
            var q = _context.AdvanceEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.EmployeeId.HasValue) q = q.Where(x => x.EmployeeId == request.EmployeeId);
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                string st = request.Status.Trim().ToUpperInvariant();
                q = q.Where(x => x.Status == st);
            }
            if (request.DeductYear.HasValue) q = q.Where(x => x.DeductYear == request.DeductYear);
            if (request.DeductMonth.HasValue) q = q.Where(x => x.DeductMonth == request.DeductMonth);

            int total = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(x => x.RequestDate)
                .Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            return new PagedResult<AdvanceDto>(await PayrollAdjustmentMaps.MapAdvances(_context, rows, ct), total, request.PageIndex, request.PageSize);
        }
    }

    public class CreateAdvanceCommand : IRequest<Guid>
    {
        public Guid EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? RequestDate { get; set; }
        public int DeductMonth { get; set; }
        public int DeductYear { get; set; }
        public string? Reason { get; set; }
        public string? Note { get; set; }
        public bool Submit { get; set; } = true;
    }

    public class CreateAdvanceCommandHandler : IRequestHandler<CreateAdvanceCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateAdvanceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateAdvanceCommand request, CancellationToken ct)
        {
            if (request.Amount <= 0) throw new InvalidOperationException("Số tiền tạm ứng phải > 0.");
            if (request.DeductMonth is < 1 or > 12) throw new InvalidOperationException("Tháng khấu trừ không hợp lệ.");
            bool empOk = await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, ct);
            if (!empOk) throw new InvalidOperationException("Nhân viên không tồn tại.");

            var entity = new AdvanceEntity
            {
                EmployeeId = request.EmployeeId,
                Amount = Math.Round(request.Amount, 0, MidpointRounding.AwayFromZero),
                RequestDate = request.RequestDate ?? DateTime.UtcNow,
                DeductMonth = request.DeductMonth,
                DeductYear = request.DeductYear,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                Status = request.Submit ? SlipStatus.Pending : SlipStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _context.AdvanceEntities.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }

    public class ReviewAdvanceCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool Approve { get; set; }
        public string? Note { get; set; }
    }

    public class ReviewAdvanceCommandHandler : IRequestHandler<ReviewAdvanceCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public ReviewAdvanceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(ReviewAdvanceCommand request, CancellationToken ct)
        {
            var entity = await _context.AdvanceEntities.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct);
            if (entity == null) return false;
            if (entity.Status is not (SlipStatus.Pending or SlipStatus.Draft))
                throw new InvalidOperationException("Chỉ duyệt được tạm ứng đang chờ.");

            entity.Status = request.Approve ? SlipStatus.Approved : SlipStatus.Rejected;
            entity.ApprovedDate = DateTime.UtcNow;
            entity.ApprovedBy = _currentUser.UserId?.ToString();
            if (!string.IsNullOrWhiteSpace(request.Note)) entity.Note = request.Note.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }

    public class CancelAdvanceCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class CancelAdvanceCommandHandler : IRequestHandler<CancelAdvanceCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CancelAdvanceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(CancelAdvanceCommand request, CancellationToken ct)
        {
            var entity = await _context.AdvanceEntities.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct);
            if (entity == null) return false;
            if (entity.Status == SlipStatus.Applied)
                throw new InvalidOperationException("Tạm ứng đã áp vào lương, không hủy được.");
            entity.Status = SlipStatus.Cancelled;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
    #endregion

    #region Deduction / Addition slips
    public class GetPayrollSlipsPagedQuery : PagedRequest, IRequest<PagedResult<PayrollSlipDto>>
    {
        public string Kind { get; set; } = SlipKind.Deduction;
        public Guid? EmployeeId { get; set; }
        public string? Status { get; set; }
        public int? ApplyYear { get; set; }
        public int? ApplyMonth { get; set; }
    }

    public class GetPayrollSlipsPagedQueryHandler : IRequestHandler<GetPayrollSlipsPagedQuery, PagedResult<PayrollSlipDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetPayrollSlipsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<PayrollSlipDto>> Handle(GetPayrollSlipsPagedQuery request, CancellationToken ct)
        {
            string kind = (request.Kind ?? SlipKind.Deduction).Trim().ToUpperInvariant();
            if (kind == SlipKind.Addition)
            {
                var q = _context.CashAdditionSlipEntities.AsNoTracking().Where(x => !x.IsDeleted);
                if (request.EmployeeId.HasValue) q = q.Where(x => x.EmployeeId == request.EmployeeId);
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    string st = request.Status.Trim().ToUpperInvariant();
                    q = q.Where(x => x.Status == st);
                }
                if (request.ApplyYear.HasValue) q = q.Where(x => x.ApplyYear == request.ApplyYear);
                if (request.ApplyMonth.HasValue) q = q.Where(x => x.ApplyMonth == request.ApplyMonth);
                int total = await q.CountAsync(ct);
                var rows = await q.OrderByDescending(x => x.AdditionDate)
                    .Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
                return new PagedResult<PayrollSlipDto>(await PayrollAdjustmentMaps.MapAdditions(_context, rows, ct), total, request.PageIndex, request.PageSize);
            }
            else
            {
                var q = _context.DeductionSlipEntities.AsNoTracking().Where(x => !x.IsDeleted);
                if (request.EmployeeId.HasValue) q = q.Where(x => x.EmployeeId == request.EmployeeId);
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    string st = request.Status.Trim().ToUpperInvariant();
                    q = q.Where(x => x.Status == st);
                }
                if (request.ApplyYear.HasValue) q = q.Where(x => x.ApplyYear == request.ApplyYear);
                if (request.ApplyMonth.HasValue) q = q.Where(x => x.ApplyMonth == request.ApplyMonth);
                int total = await q.CountAsync(ct);
                var rows = await q.OrderByDescending(x => x.DeductionDate)
                    .Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
                return new PagedResult<PayrollSlipDto>(await PayrollAdjustmentMaps.MapDeductions(_context, rows, ct), total, request.PageIndex, request.PageSize);
            }
        }
    }

    public class CreatePayrollSlipCommand : IRequest<Guid>
    {
        public string Kind { get; set; } = SlipKind.Deduction;
        public Guid EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? SlipDate { get; set; }
        public string? SlipType { get; set; }
        public int ApplyMonth { get; set; }
        public int ApplyYear { get; set; }
        public string? Reason { get; set; }
        public string? Note { get; set; }
        public bool AutoApprove { get; set; } = true;
    }

    public class CreatePayrollSlipCommandHandler : IRequestHandler<CreatePayrollSlipCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreatePayrollSlipCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreatePayrollSlipCommand request, CancellationToken ct)
        {
            if (request.Amount <= 0) throw new InvalidOperationException("Số tiền phải > 0.");
            if (request.ApplyMonth is < 1 or > 12) throw new InvalidOperationException("Tháng áp dụng không hợp lệ.");
            bool empOk = await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, ct);
            if (!empOk) throw new InvalidOperationException("Nhân viên không tồn tại.");

            string status = request.AutoApprove ? SlipStatus.Approved : SlipStatus.Pending;
            string kind = (request.Kind ?? SlipKind.Deduction).Trim().ToUpperInvariant();
            if (kind == SlipKind.Addition)
            {
                var entity = new CashAdditionSlipEntity
                {
                    EmployeeId = request.EmployeeId,
                    Amount = Math.Round(request.Amount, 0, MidpointRounding.AwayFromZero),
                    AdditionDate = request.SlipDate ?? DateTime.UtcNow,
                    AdditionType = string.IsNullOrWhiteSpace(request.SlipType)
                        ? AdditionSlipType.Bonus
                        : request.SlipType.Trim().ToUpperInvariant(),
                    ApplyMonth = request.ApplyMonth,
                    ApplyYear = request.ApplyYear,
                    Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                    Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                    Status = status,
                    ApprovedBy = request.AutoApprove ? _currentUser.UserId?.ToString() : null,
                    ApprovedDate = request.AutoApprove ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                };
                _context.CashAdditionSlipEntities.Add(entity);
                await _context.SaveChangesAsync(ct);
                return entity.Id;
            }
            else
            {
                var entity = new DeductionSlipEntity
                {
                    EmployeeId = request.EmployeeId,
                    Amount = Math.Round(request.Amount, 0, MidpointRounding.AwayFromZero),
                    DeductionDate = request.SlipDate ?? DateTime.UtcNow,
                    DeductionType = string.IsNullOrWhiteSpace(request.SlipType)
                        ? DeductionSlipType.Fine
                        : request.SlipType.Trim().ToUpperInvariant(),
                    ApplyMonth = request.ApplyMonth,
                    ApplyYear = request.ApplyYear,
                    Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                    Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                    Status = status,
                    ApprovedBy = request.AutoApprove ? _currentUser.UserId?.ToString() : null,
                    ApprovedDate = request.AutoApprove ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                };
                _context.DeductionSlipEntities.Add(entity);
                await _context.SaveChangesAsync(ct);
                return entity.Id;
            }
        }
    }

    public class ReviewPayrollSlipCommand : IRequest<bool>
    {
        public string Kind { get; set; } = SlipKind.Deduction;
        public Guid Id { get; set; }
        public bool Approve { get; set; }
    }

    public class ReviewPayrollSlipCommandHandler : IRequestHandler<ReviewPayrollSlipCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public ReviewPayrollSlipCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(ReviewPayrollSlipCommand request, CancellationToken ct)
        {
            string kind = (request.Kind ?? SlipKind.Deduction).Trim().ToUpperInvariant();
            string status = request.Approve ? SlipStatus.Approved : SlipStatus.Rejected;
            if (kind == SlipKind.Addition)
            {
                var entity = await _context.CashAdditionSlipEntities.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct);
                if (entity == null) return false;
                entity.Status = status;
                entity.ApprovedDate = DateTime.UtcNow;
                entity.ApprovedBy = _currentUser.UserId?.ToString();
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = _currentUser.UserId;
            }
            else
            {
                var entity = await _context.DeductionSlipEntities.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct);
                if (entity == null) return false;
                entity.Status = status;
                entity.ApprovedDate = DateTime.UtcNow;
                entity.ApprovedBy = _currentUser.UserId?.ToString();
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = _currentUser.UserId;
            }
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
    #endregion

    #region Mappers
    internal static class PayrollAdjustmentMaps
    {
        public static Task<List<AdvanceDto>> MapAdvances(IApplicationDbContext context, List<AdvanceEntity> rows, CancellationToken ct)
            => MapEmp(context, rows, ct, (e, emp) => new AdvanceDto
            {
                Id = e.Id,
                EmployeeId = e.EmployeeId,
                EmployeeCode = emp?.Code,
                EmployeeName = emp?.Name,
                Amount = e.Amount,
                RequestDate = e.RequestDate,
                DeductMonth = e.DeductMonth,
                DeductYear = e.DeductYear,
                Status = e.Status,
                Reason = e.Reason,
                Note = e.Note,
                ApprovedBy = e.ApprovedBy,
                ApprovedDate = e.ApprovedDate,
            });

        public static Task<List<PayrollSlipDto>> MapDeductions(IApplicationDbContext context, List<DeductionSlipEntity> rows, CancellationToken ct)
            => MapEmp(context, rows, ct, (e, emp) => new PayrollSlipDto
            {
                Id = e.Id,
                Kind = SlipKind.Deduction,
                EmployeeId = e.EmployeeId,
                EmployeeCode = emp?.Code,
                EmployeeName = emp?.Name,
                Amount = e.Amount,
                SlipDate = e.DeductionDate,
                SlipType = e.DeductionType,
                ApplyMonth = e.ApplyMonth,
                ApplyYear = e.ApplyYear,
                Status = e.Status,
                Reason = e.Reason,
                Note = e.Note,
                ApprovedBy = e.ApprovedBy,
                ApprovedDate = e.ApprovedDate,
            });

        public static Task<List<PayrollSlipDto>> MapAdditions(IApplicationDbContext context, List<CashAdditionSlipEntity> rows, CancellationToken ct)
            => MapEmp(context, rows, ct, (e, emp) => new PayrollSlipDto
            {
                Id = e.Id,
                Kind = SlipKind.Addition,
                EmployeeId = e.EmployeeId,
                EmployeeCode = emp?.Code,
                EmployeeName = emp?.Name,
                Amount = e.Amount,
                SlipDate = e.AdditionDate,
                SlipType = e.AdditionType,
                ApplyMonth = e.ApplyMonth,
                ApplyYear = e.ApplyYear,
                Status = e.Status,
                Reason = e.Reason,
                Note = e.Note,
                ApprovedBy = e.ApprovedBy,
                ApprovedDate = e.ApprovedDate,
            });

        private static async Task<List<TOut>> MapEmp<TIn, TOut>(
            IApplicationDbContext context,
            List<TIn> rows,
            CancellationToken ct,
            Func<TIn, EmpInfo?, TOut> map) where TIn : class
        {
            if (rows.Count == 0) return [];
            var empIds = rows.Select(r =>
            {
                return r switch
                {
                    AdvanceEntity a => a.EmployeeId,
                    DeductionSlipEntity d => d.EmployeeId,
                    CashAdditionSlipEntity c => c.EmployeeId,
                    _ => Guid.Empty,
                };
            }).Where(x => x != Guid.Empty).Distinct().ToList();

            var empMap = await context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => new EmpInfo(x.Code, x.FullName ?? $"{x.LastName} {x.FirstName}".Trim()),
                    ct);

            return rows.Select(r =>
            {
                Guid id = r switch
                {
                    AdvanceEntity a => a.EmployeeId,
                    DeductionSlipEntity d => d.EmployeeId,
                    CashAdditionSlipEntity c => c.EmployeeId,
                    _ => Guid.Empty,
                };
                empMap.TryGetValue(id, out var emp);
                return map(r, emp);
            }).ToList();
        }

        private record EmpInfo(string? Code, string? Name);
    }
    #endregion
}
