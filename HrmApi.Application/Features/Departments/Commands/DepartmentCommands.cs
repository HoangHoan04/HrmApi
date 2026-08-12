using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Departments.Commands
{
    #region Create Command
    public class CreateDepartmentCommand : DepartmentCommandFields, IRequest<Guid>
    {
    }

    public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateDepartmentCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            var department = new DepartmentEntity
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            DepartmentMapper.ApplyCommandFields(department, request);

            _context.DepartmentEntities.Add(department);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "DepartmentEntity",
                department.Id,
                null,
                DepartmentMapper.ToLogObject(department),
                $"Tạo mới phòng ban {department.Name} thành công");

            return department.Id;
        }

        internal static async Task ValidateAsync(
            DepartmentCommandFields request,
            Guid? excludeId,
            CancellationToken cancellationToken,
            IApplicationDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new InvalidOperationException("Mã phòng ban là bắt buộc.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên phòng ban là bắt buộc.");

            if (!request.BranchId.HasValue || request.BranchId == Guid.Empty)
                throw new InvalidOperationException("Chi nhánh là bắt buộc.");

            var branch = await context.BranchEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.BranchId.Value, cancellationToken);

            if (branch == null)
                throw new InvalidOperationException("Chi nhánh không tồn tại.");

            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
                request.CompanyId = branch.CompanyId;
            else if (branch.CompanyId.HasValue && branch.CompanyId != request.CompanyId)
                throw new InvalidOperationException("Chi nhánh không thuộc công ty đã chọn.");

            var exists = await context.DepartmentEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

            if (exists)
                throw new InvalidOperationException("Mã phòng ban đã tồn tại trong hệ thống.");

            if (request.CompanyId.HasValue)
            {
                var companyExists = await context.CompanyEntities
                    .AnyAsync(x => x.Id == request.CompanyId.Value, cancellationToken);
                if (!companyExists)
                    throw new InvalidOperationException("Công ty không tồn tại.");
            }

            if (request.BranchId.HasValue)
            {
                var branchExists = await context.BranchEntities
                    .AnyAsync(x => x.Id == request.BranchId.Value, cancellationToken);
                if (!branchExists)
                    throw new InvalidOperationException("Chi nhánh không tồn tại.");
            }

            if (request.ParentDepartmentId.HasValue)
            {
                if (excludeId.HasValue && request.ParentDepartmentId.Value == excludeId.Value)
                    throw new InvalidOperationException("Phòng ban không thể là cha của chính nó.");

                var parentExists = await context.DepartmentEntities
                    .AnyAsync(x => x.Id == request.ParentDepartmentId.Value, cancellationToken);
                if (!parentExists)
                    throw new InvalidOperationException("Phòng ban cha không tồn tại.");
            }

            if (request.ManagerId.HasValue)
            {
                var managerExists = await context.EmployeeEntities
                    .AnyAsync(x => x.Id == request.ManagerId.Value, cancellationToken);
                if (!managerExists)
                    throw new InvalidOperationException("Nhân viên quản lý không tồn tại.");
            }

            if (request.DeputyManagerId.HasValue)
            {
                var deputyExists = await context.EmployeeEntities
                    .AnyAsync(x => x.Id == request.DeputyManagerId.Value, cancellationToken);
                if (!deputyExists)
                    throw new InvalidOperationException("Nhân viên phó phòng không tồn tại.");
            }
        }
    }
    #endregion

    #region Update Command
    public class UpdateDepartmentCommand : DepartmentCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateDepartmentCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _context.DepartmentEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (department == null) return false;

            await CreateDepartmentCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            var oldValue = DepartmentMapper.ToLogObject(department);

            DepartmentMapper.ApplyCommandFields(department, request);
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var newValue = DepartmentMapper.ToLogObject(department);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "DepartmentEntity",
                department.Id,
                oldValue,
                newValue,
                $"Cập nhật thông tin phòng ban {department.Name} thành công");

            return true;
        }
    }
    #endregion
    #region Activate Command
    public class ActivateDepartmentCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivateDepartmentCommandHandler : IRequestHandler<ActivateDepartmentCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivateDepartmentCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _context.DepartmentEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (department == null) return false;

            department.IsDeleted = false;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "DepartmentEntity",
                department.Id,
                new { IsDeleted = true },
                new { IsDeleted = false },
                $"Kích hoạt phòng ban {department.Name} thành công");

            return true;
        }
    }
    #endregion
    #region Deactivate Command
    public class DeactivateDepartmentCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateDepartmentCommandHandler : IRequestHandler<DeactivateDepartmentCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivateDepartmentCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _context.DepartmentEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (department == null) return false;

            department.IsDeleted = true;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "DepartmentEntity",
                department.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                $"Ngưng hoạt động phòng ban {department.Name} thành công");

            return true;
        }
    }
    #endregion
}
