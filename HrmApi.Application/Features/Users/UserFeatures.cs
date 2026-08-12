using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Role;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Users
{
    public class GetUserByIdQuery : IRequest<UserDetailDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetUserByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<UserDetailDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var row = await (
                from u in _context.UserEntities.AsNoTracking()
                join c in _context.CompanyEntities.AsNoTracking() on u.CompanyId equals c.Id into cj
                from c in cj.DefaultIfEmpty()
                join e in _context.EmployeeEntities.AsNoTracking() on u.EmployeeId equals e.Id into ej
                from e in ej.DefaultIfEmpty()
                where u.Id == request.Id && !u.IsDeleted
                select new
                {
                    u.Id,
                    u.Username,
                    u.Type,
                    u.Email,
                    u.PhoneNumber,
                    u.IsActive,
                    u.IsLocked,
                    u.CompanyId,
                    u.BranchId,
                    CompanyCode = c != null ? c.Code : null,
                    CompanyName = c != null ? c.Name : null,
                    u.EmployeeId,
                    EmployeeCode = e != null ? e.Code : null,
                    EmployeeName = e != null
                        ? (!string.IsNullOrWhiteSpace(e.FullName)
                            ? e.FullName!
                            : (e.LastName + " " + e.FirstName).Trim())
                        : null,
                    u.MustChangePassword,
                    u.LastLoginAt,
                }
            ).FirstOrDefaultAsync(cancellationToken);

            if (row == null) return null;

            var roles = await (
                from ur in _context.UserRoleEntities.AsNoTracking()
                join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                where ur.UserId == row.Id && !ur.IsDeleted && !r.IsDeleted
                select new { ur.RoleId, r.Code }
            ).ToListAsync(cancellationToken);

            return new UserDetailDto
            {
                Id = row.Id,
                Username = row.Username,
                Type = row.Type,
                Email = row.Email,
                PhoneNumber = row.PhoneNumber,
                IsActive = row.IsActive,
                IsLocked = row.IsLocked,
                CompanyId = row.CompanyId,
                BranchId = row.BranchId,
                CompanyCode = row.CompanyCode,
                CompanyName = row.CompanyName,
                EmployeeId = row.EmployeeId,
                EmployeeCode = row.EmployeeCode,
                EmployeeName = row.EmployeeName,
                MustChangePassword = row.MustChangePassword,
                LastLoginAt = row.LastLoginAt,
                RoleIds = roles.Select(x => x.RoleId).Distinct().ToList(),
                RoleCodes = roles.Select(x => x.Code).Distinct().OrderBy(c => c).ToList(),
            };
        }
    }

    public class CreateUserCommand : IRequest<Guid>
    {
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string Type { get; set; } = "EMPLOYEE";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool MustChangePassword { get; set; } = true;
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IPasswordHasherService _passwordHasher;

        public CreateUserCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog,
            IPasswordHasherService passwordHasher)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _passwordHasher = passwordHasher;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            string username = (request.Username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Tên đăng nhập không được để trống.");

            string type = (request.Type ?? "EMPLOYEE").Trim().ToUpperInvariant();
            if (type is not ("ADMIN" or "EMPLOYEE" or "HR" or "MANAGER"))
                type = "EMPLOYEE";

            bool exists = await _context.UserEntities.AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.Username.ToLower() == username.ToLower(), cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Tên đăng nhập '{username}' đã tồn tại.");

            Guid? companyId = request.CompanyId;
            Guid? branchId = request.BranchId;
            string? email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            string? phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            if (request.EmployeeId.HasValue)
            {
                bool employeeTaken = await _context.UserEntities.AsNoTracking()
                    .AnyAsync(x => !x.IsDeleted && x.EmployeeId == request.EmployeeId, cancellationToken);
                if (employeeTaken)
                    throw new InvalidOperationException("Nhân viên này đã có tài khoản.");

                var employee = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == request.EmployeeId && !x.IsDeleted)
                    .Select(x => new { x.Id, x.CompanyId, x.BranchId, x.Email, x.Phone, x.Code })
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");

                companyId ??= employee.CompanyId;
                branchId ??= employee.BranchId;
                email ??= employee.Email;
                phone ??= employee.Phone;
            }

            if (companyId.HasValue)
            {
                bool companyOk = await _context.CompanyEntities.AsNoTracking()
                    .AnyAsync(x => x.Id == companyId && !x.IsDeleted, cancellationToken);
                if (!companyOk)
                    throw new InvalidOperationException("Công ty không hợp lệ.");
            }

            string password = string.IsNullOrWhiteSpace(request.Password) ? "123@123@" : request.Password.Trim();
            var user = new UserEntity
            {
                Username = username,
                Type = type,
                Email = email,
                PhoneNumber = phone,
                EmployeeId = request.EmployeeId,
                CompanyId = companyId,
                BranchId = branchId,
                IsActive = request.IsActive,
                IsLocked = false,
                MustChangePassword = request.MustChangePassword,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _context.UserEntities.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "UserEntity",
                user.Id,
                null,
                new { user.Username, user.Type, user.EmployeeId, user.CompanyId },
                "Tạo tài khoản");

            return user.Id;
        }
    }

    public class UpdateUserCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Type { get; set; } = "EMPLOYEE";
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public UpdateUserCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.UserEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");

            string type = (request.Type ?? user.Type).Trim().ToUpperInvariant();
            if (type is not ("ADMIN" or "EMPLOYEE" or "HR" or "MANAGER"))
                type = user.Type;

            if (request.EmployeeId.HasValue && request.EmployeeId != user.EmployeeId)
            {
                bool employeeTaken = await _context.UserEntities.AsNoTracking()
                    .AnyAsync(x => !x.IsDeleted && x.EmployeeId == request.EmployeeId && x.Id != user.Id, cancellationToken);
                if (employeeTaken)
                    throw new InvalidOperationException("Nhân viên này đã có tài khoản.");

                bool employeeExists = await _context.EmployeeEntities.AsNoTracking()
                    .AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken);
                if (!employeeExists)
                    throw new InvalidOperationException("Không tìm thấy nhân viên.");
            }

            var before = new { user.Email, user.PhoneNumber, user.Type, user.IsActive, user.IsLocked, user.EmployeeId, user.CompanyId };

            user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            user.Type = type;
            user.EmployeeId = request.EmployeeId;
            user.CompanyId = request.CompanyId;
            user.BranchId = request.BranchId;
            user.IsActive = request.IsActive;
            user.IsLocked = request.IsLocked;
            if (!request.IsLocked)
            {
                user.LockedUntil = null;
                user.FailedLoginAttempts = 0;
            }
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUser.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "UserEntity",
                user.Id,
                before,
                new { user.Email, user.PhoneNumber, user.Type, user.IsActive, user.IsLocked, user.EmployeeId, user.CompanyId },
                "Cập nhật tài khoản");

            return true;
        }
    }

    public class ResetUserPasswordCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? NewPassword { get; set; }
        public bool MustChangePassword { get; set; } = true;
    }

    public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IPasswordHasherService _passwordHasher;

        public ResetUserPasswordCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog,
            IPasswordHasherService passwordHasher)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.UserEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");

            string password = string.IsNullOrWhiteSpace(request.NewPassword) ? "123@123@" : request.NewPassword.Trim();
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            user.MustChangePassword = request.MustChangePassword;
            user.PasswordChangedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUser.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "UserEntity",
                user.Id,
                null,
                new { Reset = true, user.MustChangePassword },
                "Reset mật khẩu tài khoản");

            return true;
        }
    }

    public class DeleteUserCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public DeleteUserCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.UserEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");

            if (_currentUser.UserId == user.Id)
                throw new InvalidOperationException("Không thể xóa tài khoản đang đăng nhập.");

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUser.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DELETE,
                "UserEntity",
                user.Id,
                new { user.Username },
                null,
                "Xóa tài khoản");

            return true;
        }
    }
}
