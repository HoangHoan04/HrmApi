using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Parts.Commands
{
    #region Create Command
    public class CreatePartCommand : PartCommandFields, IRequest<Guid>
    {
    }

    public class CreatePartCommandHandler : IRequestHandler<CreatePartCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreatePartCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreatePartCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            PartEntity part = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            PartMapper.ApplyCommandFields(part, request);

            _ = _context.PartEntities.Add(part);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "PartEntity",
                part.Id,
                null,
                PartMapper.ToLogObject(part),
                "Tạo mới bộ phận " + (part.Name ?? part.Code) + " thành công");

            return part.Id;
        }

        internal static async Task ValidateAsync(
            PartCommandFields request,
            Guid? excludeId,
            CancellationToken cancellationToken,
            IApplicationDbContext context)
        {
            if (!request.PartMasterId.HasValue)
            {
                throw new InvalidOperationException("Mẫu bộ phận (PartMaster) là bắt buộc.");
            }

            bool partMasterExists = await context.PartMasterEntities
                .AnyAsync(x => x.Id == request.PartMasterId.Value, cancellationToken);

            if (!partMasterExists)
            {
                throw new InvalidOperationException("Mẫu bộ phận không tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                string code = request.Code.Trim().ToLower();
                bool exists = await context.PartEntities
                    .AnyAsync(x => x.Code != null && x.Code.ToLower() == code
                        && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

                if (exists)
                {
                    throw new InvalidOperationException("Mã bộ phận đã tồn tại trong hệ thống.");
                }
            }

            if (request.DepartmentId.HasValue)
            {
                bool departmentExists = await context.DepartmentEntities
                    .AnyAsync(x => x.Id == request.DepartmentId.Value, cancellationToken);

                if (!departmentExists)
                {
                    throw new InvalidOperationException("Phòng ban không tồn tại.");
                }
            }

            if (request.CompanyId.HasValue)
            {
                bool companyExists = await context.CompanyEntities
                    .AnyAsync(x => x.Id == request.CompanyId.Value, cancellationToken);

                if (!companyExists)
                {
                    throw new InvalidOperationException("Công ty không tồn tại.");
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
    }
    #endregion

    #region Update Command
    public class UpdatePartCommand : PartCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePartCommandHandler : IRequestHandler<UpdatePartCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdatePartCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
        {
            var part = await _context.PartEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (part == null)
            {
                return false;
            }

            await CreatePartCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            object oldValue = PartMapper.ToLogObject(part);

            PartMapper.ApplyCommandFields(part, request);
            part.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            object newValue = PartMapper.ToLogObject(part);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "PartEntity",
                part.Id,
                oldValue,
                newValue,
                "Cập nhật thông tin bộ phận " + (part.Name ?? part.Code) + " thành công");

            return true;
        }
    }
    #endregion

    #region Activate Command
    public class ActivatePartCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivatePartCommandHandler : IRequestHandler<ActivatePartCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivatePartCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivatePartCommand request, CancellationToken cancellationToken)
        {
            var part = await _context.PartEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (part == null)
            {
                return false;
            }

            part.IsDeleted = false;
            part.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "PartEntity",
                part.Id,
                new { IsDeleted = true },
                new { IsDeleted = false },
                "Kích hoạt bộ phận " + (part.Name ?? part.Code) + " thành công");

            return true;
        }
    }
    #endregion

    #region Deactivate Command
    public class DeactivatePartCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivatePartCommandHandler : IRequestHandler<DeactivatePartCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivatePartCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivatePartCommand request, CancellationToken cancellationToken)
        {
            var part = await _context.PartEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (part == null)
            {
                return false;
            }

            part.IsDeleted = true;
            part.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "PartEntity",
                part.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Ngưng hoạt động bộ phận " + (part.Name ?? part.Code) + " thành công");

            return true;
        }
    }
    #endregion
}
