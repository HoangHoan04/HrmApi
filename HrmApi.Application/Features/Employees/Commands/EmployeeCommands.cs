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
        private readonly IPasswordHasherService _passwordHasher;

        public CreateEmployeeCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            IPasswordHasherService passwordHasher)
        {
            _context = context;
            _actionLog = actionLog;
            _passwordHasher = passwordHasher;
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

            const string defaultPassword = "123@123@";
            string usernameNormalized = employee.Code.Trim().ToLower();
            bool userExists = await _context.UserEntities
                .AnyAsync(x => x.Username.ToLower() == usernameNormalized, cancellationToken);

            if (!userExists)
            {
                UserEntity user = new()
                {
                    EmployeeId = employee.Id,
                    Username = employee.Code.Trim(),
                    Email = employee.Email,
                    PhoneNumber = employee.Phone,
                    Type = "EMPLOYEE",
                    IsActive = true,
                    IsLocked = false,
                    MustChangePassword = true,
                    CreatedAt = DateTime.UtcNow,
                    CompanyId = employee.CompanyId,
                    BranchId = employee.BranchId,
                };
                user.PasswordHash = _passwordHasher.HashPassword(user, defaultPassword);

                _ = _context.UserEntities.Add(user);
                _ = await _context.SaveChangesAsync(cancellationToken);
            }

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "EmployeeEntity",
                employee.Id,
                null,
                EmployeeMapper.ToLogObject(employee),
                "Tạo mới nhân viên " + employee.FullName + " thành công");

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
            var employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (employee == null)
            {
                return false;
            }

            await CreateEmployeeCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            object oldValue = EmployeeMapper.ToLogObject(employee);

            EmployeeMapper.ApplyCommandFields(employee, request);
            employee.UpdatedAt = DateTime.UtcNow;

            var user = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.EmployeeId == employee.Id && !u.IsDeleted, cancellationToken);
            if (user != null)
            {
                user.CompanyId = employee.CompanyId;
                user.BranchId = employee.BranchId;
                user.Email = employee.Email;
                user.PhoneNumber = employee.Phone;
                user.UpdatedAt = DateTime.UtcNow;
            }

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
            var employee = await _context.EmployeeEntities
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
            var employee = await _context.EmployeeEntities
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
