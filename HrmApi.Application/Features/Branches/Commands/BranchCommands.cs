using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Branches.Commands
{
    #region Create Command
    public class CreateBranchCommand : BranchCommandFields, IRequest<Guid>
    {
    }

    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateBranchCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            BranchEntity branch = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            BranchMapper.ApplyCommandFields(branch, request);
            await SyncManagerInfoAsync(branch, request, _context, cancellationToken);

            _ = _context.BranchEntities.Add(branch);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "BranchEntity",
                branch.Id,
                null,
                BranchMapper.ToLogObject(branch),
                "Tạo mới chi nhánh " + branch.Name);

            return branch.Id;
        }

        internal static async Task ValidateAsync(
            BranchCommandFields request,
            Guid? excludeId,
            CancellationToken cancellationToken,
            IApplicationDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã chi nhánh là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên chi nhánh là bắt buộc.");
            }

            bool exists = await context.BranchEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("Mã chi nhánh đã tồn tại trong hệ thống.");
            }

            if (request.ParentBranchId.HasValue)
            {
                if (excludeId.HasValue && request.ParentBranchId.Value == excludeId.Value)
                {
                    throw new InvalidOperationException("Chi nhánh không thể là chi nhánh mẹ của chính nó.");
                }

                bool parentExists = await context.BranchEntities
                    .AnyAsync(x => x.Id == request.ParentBranchId.Value, cancellationToken);

                if (!parentExists)
                {
                    throw new InvalidOperationException("Chi nhánh mẹ không tồn tại.");
                }
            }

            if (request.ManagerId.HasValue)
            {
                bool managerExists = await context.EmployeeEntities
                    .AnyAsync(x => x.Id == request.ManagerId.Value && !x.IsDeleted, cancellationToken);
                if (!managerExists)
                {
                    throw new InvalidOperationException("Nhân viên quản lý không tồn tại.");
                }
            }
        }

        internal static async Task SyncManagerInfoAsync(
            BranchEntity branch,
            BranchCommandFields request,
            IApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            if (!request.ManagerId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(request.ManagerName))
                {
                    branch.ManagerName = null;
                }

                if (string.IsNullOrWhiteSpace(request.ManagerPhone))
                {
                    branch.ManagerPhone = null;
                }

                return;
            }

            var employee = await context.EmployeeEntities
                .AsNoTracking()
                .Where(x => x.Id == request.ManagerId.Value)
                .Select(x => new
                {
                    Name = x.FullName ?? ((x.FirstName ?? string.Empty) + " " + (x.LastName ?? string.Empty)).Trim(),
                    x.Phone
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (employee == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.ManagerName))
            {
                branch.ManagerName = string.IsNullOrWhiteSpace(employee.Name) ? null : employee.Name;
            }

            if (string.IsNullOrWhiteSpace(request.ManagerPhone))
            {
                branch.ManagerPhone = string.IsNullOrWhiteSpace(employee.Phone) ? null : employee.Phone;
            }
        }
    }
    #endregion

    #region Update Command
    public class UpdateBranchCommand : BranchCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateBranchCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
        {
            BranchEntity? branch = await _context.BranchEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (branch == null)
            {
                return false;
            }

            await CreateBranchCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            object oldValue = BranchMapper.ToLogObject(branch);

            BranchMapper.ApplyCommandFields(branch, request);
            await CreateBranchCommandHandler.SyncManagerInfoAsync(branch, request, _context, cancellationToken);
            branch.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            object newValue = BranchMapper.ToLogObject(branch);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "BranchEntity",
                branch.Id,
                oldValue,
                newValue,
                "Cập nhật chi nhánh " + branch.Name);

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
        private readonly IActionLogService _actionLog;

        public ActivateBranchCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateBranchCommand request, CancellationToken cancellationToken)
        {
            BranchEntity? branch = await _context.BranchEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (branch == null)
            {
                return false;
            }

            branch.IsDeleted = false;
            branch.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "BranchEntity",
                branch.Id,
                new { IsDeleted = true },
                new { IsDeleted = false },
                "Kích hoạt chi nhánh " + branch.Name + " thành công");

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
        private readonly IActionLogService _actionLog;

        public DeactivateBranchCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateBranchCommand request, CancellationToken cancellationToken)
        {
            BranchEntity? branch = await _context.BranchEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (branch == null)
            {
                return false;
            }

            branch.IsDeleted = true;
            branch.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "BranchEntity",
                branch.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Ngưng hoạt động chi nhánh " + branch.Name + " thành công");

            return true;
        }
    }
    #endregion
}
