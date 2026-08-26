using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Employees.Commands
{
    #region Create Command
    public class CreateEmployeeCommand : EmployeeCommandFields, IRequest<Guid>
    {
    }

    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuthProvisioningService _authProvisioning;

        public CreateEmployeeCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser,
            IAuthProvisioningService authProvisioning)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
            _authProvisioning = authProvisioning;
        }

        public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            EmployeeEntity employee = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            EmployeeMapper.ApplyCommandFields(employee, request);

            _ = _context.EmployeeEntities.Add(employee);
            _ = await _context.SaveChangesAsync(cancellationToken);

            bool hasEmployeeRole = await _context.RoleEntities.AsNoTracking()
                .AnyAsync(x => x.Code == RoleCodes.Employee && !x.IsDeleted && x.IsActive, cancellationToken);
            if (hasEmployeeRole)
            {
                Guid employeeRoleId = await _context.RoleEntities.AsNoTracking()
                    .Where(x => x.Code == RoleCodes.Employee && !x.IsDeleted && x.IsActive)
                    .Select(x => x.Id)
                    .FirstAsync(cancellationToken);

                _ = _context.UserRoleEntities.Add(new UserRoleEntity
                {
                    EmployeeId = employee.Id,
                    RoleId = employeeRoleId,
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                });
                _ = await _context.SaveChangesAsync(cancellationToken);
            }

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "EmployeeEntity",
                employee.Id,
                null,
                EmployeeMapper.ToLogObject(employee),
                "Tạo mới nhân viên " + employee.FullName + " thành công");

            // Tạo tài khoản Auth cho nhân viên mới
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var userId = await _authProvisioning.ProvisionEmployeeAccountAsync(
                    employee.Email,
                    employee.FullName ?? employee.Code,
                    employee.Phone,
                    null,
                    cancellationToken);

                if (userId != Guid.Empty)
                {
                    employee.UserId = userId;
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            return employee.Id;
        }

        internal static async Task ValidateAsync(
            EmployeeCommandFields request,
            Guid? excludeId,
            CancellationToken cancellationToken,
            IApplicationDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã nhân viên là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                throw new InvalidOperationException("Họ nhân viên là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                throw new InvalidOperationException("Tên nhân viên là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                throw new InvalidOperationException("Số điện thoại là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new InvalidOperationException("Email là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.IdentityCard))
            {
                throw new InvalidOperationException("Số CCCD là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.PlaceOfIsssuance))
            {
                throw new InvalidOperationException("Nơi cấp CCCD là bắt buộc.");
            }

            if (request.DayOfBirth == default)
            {
                throw new InvalidOperationException("Ngày sinh là bắt buộc.");
            }

            if (request.IssuanceDate == default)
            {
                throw new InvalidOperationException("Ngày cấp CCCD là bắt buộc.");
            }

            if (request.JoinDate == default)
            {
                throw new InvalidOperationException("Ngày vào làm là bắt buộc.");
            }

            string code = request.Code.Trim().ToLower();
            bool codeExists = await context.EmployeeEntities
                .AnyAsync(x => x.Code.ToLower() == code
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

            if (codeExists)
            {
                throw new InvalidOperationException("Mã nhân viên đã tồn tại trong hệ thống.");
            }

            string identityCard = request.IdentityCard.Trim().ToLower();
            bool identityExists = await context.EmployeeEntities
                .AnyAsync(x => x.IdentityCard.ToLower() == identityCard
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

            if (identityExists)
            {
                throw new InvalidOperationException("Số CCCD đã tồn tại trong hệ thống.");
            }

            if (request.DirectManagerId.HasValue && request.DirectManagerId != Guid.Empty)
            {
                if (excludeId.HasValue && request.DirectManagerId == excludeId.Value)
                {
                    throw new InvalidOperationException("Quản lý trực tiếp không thể là chính nhân viên.");
                }

                bool managerExists = await context.EmployeeEntities
                    .AnyAsync(x => x.Id == request.DirectManagerId && !x.IsDeleted, cancellationToken);
                if (!managerExists)
                {
                    throw new InvalidOperationException("Quản lý trực tiếp không tồn tại.");
                }
            }
        }
    }
    #endregion

    #region Update Command
    public class UpdateEmployeeCommand : EmployeeCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateEmployeeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            EmployeeEntity? employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (employee == null)
            {
                return false;
            }

            await CreateEmployeeCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            object oldValue = EmployeeMapper.ToLogObject(employee);

            EmployeeMapper.ApplyCommandFields(employee, request);
            employee.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            object newValue = EmployeeMapper.ToLogObject(employee);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "EmployeeEntity",
                employee.Id,
                oldValue,
                newValue,
                "Cập nhật thông tin nhân viên " + employee.FullName + " thành công");

            return true;
        }
    }
    #endregion

    #region Activate Command
    public class ActivateEmployeeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivateEmployeeCommandHandler : IRequestHandler<ActivateEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivateEmployeeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateEmployeeCommand request, CancellationToken cancellationToken)
        {
            EmployeeEntity? employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (employee == null)
            {
                return false;
            }

            employee.IsDeleted = false;
            employee.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "EmployeeEntity",
                employee.Id,
                new { IsDeleted = true },
                new { IsDeleted = false },
                "Kích hoạt nhân viên " + employee.FullName + " thành công");

            return true;
        }
    }
    #endregion

    #region Deactivate Command
    public class DeactivateEmployeeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateEmployeeCommandHandler : IRequestHandler<DeactivateEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivateEmployeeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
        {
            EmployeeEntity? employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (employee == null)
            {
                return false;
            }

            employee.IsDeleted = true;
            employee.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "EmployeeEntity",
                employee.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Ngưng hoạt động nhân viên " + employee.FullName + " thành công");

            return true;
        }
    }
    #endregion
}
