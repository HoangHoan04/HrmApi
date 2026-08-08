using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Employees.Commands
{
    #region Dependent
    public class EmployeeDependentFields
    {
        public Guid EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public DateTime? DayOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IdentityNumber { get; set; }
        public string? TaxCode { get; set; }
        public DateTime? DependentFromDate { get; set; }
        public DateTime? DependentToDate { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class CreateEmployeeDependentCommand : EmployeeDependentFields, IRequest<Guid> { }

    public class UpdateEmployeeDependentCommand : EmployeeDependentFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteEmployeeDependentCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class CreateEmployeeDependentCommandHandler : IRequestHandler<CreateEmployeeDependentCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateEmployeeDependentCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateEmployeeDependentCommand request, CancellationToken cancellationToken)
        {
            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            EmployeeChildHelpers.ValidateDependent(request);

            var entity = new EmployeeDependentEntity
            {
                EmployeeId = request.EmployeeId,
                FullName = request.FullName.Trim(),
                Relationship = request.Relationship.Trim(),
                DayOfBirth = request.DayOfBirth,
                Gender = EmployeeChildHelpers.TrimOrNull(request.Gender),
                IdentityNumber = EmployeeChildHelpers.TrimOrNull(request.IdentityNumber),
                TaxCode = EmployeeChildHelpers.TrimOrNull(request.TaxCode),
                DependentFromDate = request.DependentFromDate,
                DependentToDate = request.DependentToDate,
                Status = EmployeeChildHelpers.TrimOrNull(request.Status),
                Note = EmployeeChildHelpers.TrimOrNull(request.Note),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeDependentEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "EmployeeEntity",
                entity.Id,
                null,
                EmployeeMapper.ToDependentDto(entity),
                "Thêm người phụ thuộc " + entity.FullName);

            return entity.Id;
        }
    }

    public class UpdateEmployeeDependentCommandHandler : IRequestHandler<UpdateEmployeeDependentCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateEmployeeDependentCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateEmployeeDependentCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeDependentEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            EmployeeChildHelpers.ValidateDependent(request);

            var oldValue = EmployeeMapper.ToDependentDto(entity);
            entity.EmployeeId = request.EmployeeId;
            entity.FullName = request.FullName.Trim();
            entity.Relationship = request.Relationship.Trim();
            entity.DayOfBirth = request.DayOfBirth;
            entity.Gender = EmployeeChildHelpers.TrimOrNull(request.Gender);
            entity.IdentityNumber = EmployeeChildHelpers.TrimOrNull(request.IdentityNumber);
            entity.TaxCode = EmployeeChildHelpers.TrimOrNull(request.TaxCode);
            entity.DependentFromDate = request.DependentFromDate;
            entity.DependentToDate = request.DependentToDate;
            entity.Status = EmployeeChildHelpers.TrimOrNull(request.Status);
            entity.Note = EmployeeChildHelpers.TrimOrNull(request.Note);
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "EmployeeEntity",
                entity.Id,
                oldValue,
                EmployeeMapper.ToDependentDto(entity),
                "Cập nhật người phụ thuộc " + entity.FullName);

            return true;
        }
    }

    public class DeleteEmployeeDependentCommandHandler : IRequestHandler<DeleteEmployeeDependentCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeleteEmployeeDependentCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteEmployeeDependentCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeDependentEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "EmployeeEntity",
                entity.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Xóa người phụ thuộc " + entity.FullName);

            return true;
        }
    }
    #endregion

    #region Education
    public class EmployeeEducationFields
    {
        public Guid EmployeeId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public string? Major { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Gpa { get; set; }
    }

    public class CreateEmployeeEducationCommand : EmployeeEducationFields, IRequest<Guid> { }

    public class UpdateEmployeeEducationCommand : EmployeeEducationFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteEmployeeEducationCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class CreateEmployeeEducationCommandHandler : IRequestHandler<CreateEmployeeEducationCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateEmployeeEducationCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateEmployeeEducationCommand request, CancellationToken cancellationToken)
        {
            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            if (string.IsNullOrWhiteSpace(request.SchoolName))
                throw new InvalidOperationException("Tên trường là bắt buộc.");

            var entity = new EmployeeEducationEntity
            {
                EmployeeId = request.EmployeeId,
                SchoolName = request.SchoolName.Trim(),
                Degree = EmployeeChildHelpers.TrimOrNull(request.Degree),
                Major = EmployeeChildHelpers.TrimOrNull(request.Major),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Gpa = EmployeeChildHelpers.TrimOrNull(request.Gpa),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeEducationEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "EmployeeEntity",
                entity.Id,
                null,
                EmployeeMapper.ToEducationDto(entity),
                "Thêm học vấn " + entity.SchoolName);

            return entity.Id;
        }
    }

    public class UpdateEmployeeEducationCommandHandler : IRequestHandler<UpdateEmployeeEducationCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateEmployeeEducationCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateEmployeeEducationCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeEducationEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            if (string.IsNullOrWhiteSpace(request.SchoolName))
                throw new InvalidOperationException("Tên trường là bắt buộc.");

            var oldValue = EmployeeMapper.ToEducationDto(entity);
            entity.EmployeeId = request.EmployeeId;
            entity.SchoolName = request.SchoolName.Trim();
            entity.Degree = EmployeeChildHelpers.TrimOrNull(request.Degree);
            entity.Major = EmployeeChildHelpers.TrimOrNull(request.Major);
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.Gpa = EmployeeChildHelpers.TrimOrNull(request.Gpa);
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "EmployeeEducationEntity",
                entity.Id,
                oldValue,
                EmployeeMapper.ToEducationDto(entity),
                "Cập nhật học vấn " + entity.SchoolName);

            return true;
        }
    }

    public class DeleteEmployeeEducationCommandHandler : IRequestHandler<DeleteEmployeeEducationCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeleteEmployeeEducationCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteEmployeeEducationCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeEducationEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "EmployeeEntity",
                entity.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Xóa học vấn " + entity.SchoolName);

            return true;
        }
    }
    #endregion

    #region Certificate
    public class EmployeeCertificateFields
    {
        public Guid EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IssuingOrganization { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CredentialId { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreateEmployeeCertificateCommand : EmployeeCertificateFields, IRequest<Guid> { }

    public class UpdateEmployeeCertificateCommand : EmployeeCertificateFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteEmployeeCertificateCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class CreateEmployeeCertificateCommandHandler : IRequestHandler<CreateEmployeeCertificateCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateEmployeeCertificateCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateEmployeeCertificateCommand request, CancellationToken cancellationToken)
        {
            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên chứng chỉ là bắt buộc.");

            var entity = new EmployeeCertificateEntity
            {
                EmployeeId = request.EmployeeId,
                Name = request.Name.Trim(),
                IssuingOrganization = EmployeeChildHelpers.TrimOrNull(request.IssuingOrganization),
                IssueDate = request.IssueDate,
                ExpiryDate = request.ExpiryDate,
                CredentialId = EmployeeChildHelpers.TrimOrNull(request.CredentialId),
                ImageUrl = EmployeeChildHelpers.TrimOrNull(request.ImageUrl),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeCertificateEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "EmployeeEntity",
                entity.Id,
                null,
                EmployeeMapper.ToCertificateDto(entity),
                "Thêm chứng chỉ " + entity.Name);

            return entity.Id;
        }
    }

    public class UpdateEmployeeCertificateCommandHandler : IRequestHandler<UpdateEmployeeCertificateCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateEmployeeCertificateCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateEmployeeCertificateCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeCertificateEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên chứng chỉ là bắt buộc.");

            var oldValue = EmployeeMapper.ToCertificateDto(entity);
            entity.EmployeeId = request.EmployeeId;
            entity.Name = request.Name.Trim();
            entity.IssuingOrganization = EmployeeChildHelpers.TrimOrNull(request.IssuingOrganization);
            entity.IssueDate = request.IssueDate;
            entity.ExpiryDate = request.ExpiryDate;
            entity.CredentialId = EmployeeChildHelpers.TrimOrNull(request.CredentialId);
            entity.ImageUrl = EmployeeChildHelpers.TrimOrNull(request.ImageUrl);
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "EmployeeEntity",
                entity.Id,
                oldValue,
                EmployeeMapper.ToCertificateDto(entity),
                "Cập nhật chứng chỉ " + entity.Name);

            return true;
        }
    }

    public class DeleteEmployeeCertificateCommandHandler : IRequestHandler<DeleteEmployeeCertificateCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeleteEmployeeCertificateCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteEmployeeCertificateCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeCertificateEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "EmployeeEntity",
                entity.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Xóa chứng chỉ " + entity.Name);

            return true;
        }
    }
    #endregion

    #region File
    public class EmployeeFileFields
    {
        public Guid EmployeeId { get; set; }
        public string FileCategory { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public string? Description { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class CreateEmployeeFileCommand : EmployeeFileFields, IRequest<Guid> { }

    public class UpdateEmployeeFileCommand : EmployeeFileFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteEmployeeFileCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class CreateEmployeeFileCommandHandler : IRequestHandler<CreateEmployeeFileCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateEmployeeFileCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateEmployeeFileCommand request, CancellationToken cancellationToken)
        {
            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            if (string.IsNullOrWhiteSpace(request.FileCategory))
                throw new InvalidOperationException("Loại tài liệu là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.FileName))
                throw new InvalidOperationException("Tên file là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.FileUrl))
                throw new InvalidOperationException("URL file là bắt buộc.");

            var entity = new EmployeeFileEntity
            {
                EmployeeId = request.EmployeeId,
                FileCategory = request.FileCategory.Trim(),
                FileName = request.FileName.Trim(),
                FileUrl = request.FileUrl.Trim(),
                ContentType = EmployeeChildHelpers.TrimOrNull(request.ContentType),
                FileSize = request.FileSize,
                Description = EmployeeChildHelpers.TrimOrNull(request.Description),
                ExpiryDate = request.ExpiryDate,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeFileEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "EmployeeFileEntity",
                entity.Id,
                null,
                EmployeeMapper.ToFileDto(entity),
                "Thêm tài liệu " + entity.FileName);

            return entity.Id;
        }
    }

    public class UpdateEmployeeFileCommandHandler : IRequestHandler<UpdateEmployeeFileCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateEmployeeFileCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateEmployeeFileCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeFileEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            if (string.IsNullOrWhiteSpace(request.FileCategory))
                throw new InvalidOperationException("Loại tài liệu là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.FileName))
                throw new InvalidOperationException("Tên file là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.FileUrl))
                throw new InvalidOperationException("URL file là bắt buộc.");

            var oldValue = EmployeeMapper.ToFileDto(entity);
            entity.EmployeeId = request.EmployeeId;
            entity.FileCategory = request.FileCategory.Trim();
            entity.FileName = request.FileName.Trim();
            entity.FileUrl = request.FileUrl.Trim();
            entity.ContentType = EmployeeChildHelpers.TrimOrNull(request.ContentType);
            entity.FileSize = request.FileSize;
            entity.Description = EmployeeChildHelpers.TrimOrNull(request.Description);
            entity.ExpiryDate = request.ExpiryDate;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "EmployeeFileEntity",
                entity.Id,
                oldValue,
                EmployeeMapper.ToFileDto(entity),
                "Cập nhật tài liệu " + entity.FileName);

            return true;
        }
    }

    public class DeleteEmployeeFileCommandHandler : IRequestHandler<DeleteEmployeeFileCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeleteEmployeeFileCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteEmployeeFileCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeFileEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "EmployeeFileEntity",
                entity.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Xóa tài liệu " + entity.FileName);

            return true;
        }
    }
    #endregion

    #region Salary History
    public class EmployeeSalaryHistoryFields
    {
        public Guid EmployeeId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal? OldBasicSalary { get; set; }
        public decimal NewBasicSalary { get; set; }
        public decimal? Allowance { get; set; }
        public string? ChangeType { get; set; }
        public string? Reason { get; set; }
        public string? DecisionNumber { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Note { get; set; }
    }

    public class CreateEmployeeSalaryHistoryCommand : EmployeeSalaryHistoryFields, IRequest<Guid> { }

    public class UpdateEmployeeSalaryHistoryCommand : EmployeeSalaryHistoryFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteEmployeeSalaryHistoryCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class CreateEmployeeSalaryHistoryCommandHandler : IRequestHandler<CreateEmployeeSalaryHistoryCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateEmployeeSalaryHistoryCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateEmployeeSalaryHistoryCommand request, CancellationToken cancellationToken)
        {
            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            EmployeeChildHelpers.ValidateSalaryHistory(request);

            var entity = new EmployeeSalaryHistoryEntity
            {
                EmployeeId = request.EmployeeId,
                EffectiveDate = request.EffectiveDate,
                OldBasicSalary = request.OldBasicSalary,
                NewBasicSalary = request.NewBasicSalary,
                Allowance = request.Allowance,
                ChangeType = EmployeeChildHelpers.TrimOrNull(request.ChangeType),
                Reason = EmployeeChildHelpers.TrimOrNull(request.Reason),
                DecisionNumber = EmployeeChildHelpers.TrimOrNull(request.DecisionNumber),
                ApprovedBy = EmployeeChildHelpers.TrimOrNull(request.ApprovedBy),
                Note = EmployeeChildHelpers.TrimOrNull(request.Note),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeSalaryHistoryEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "EmployeeEntity",
                entity.Id,
                null,
                EmployeeMapper.ToSalaryHistoryDto(entity),
                "Thêm lịch sử lương hiệu lực " + entity.EffectiveDate.ToString("yyyy-MM-dd"));

            return entity.Id;
        }
    }

    public class UpdateEmployeeSalaryHistoryCommandHandler : IRequestHandler<UpdateEmployeeSalaryHistoryCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateEmployeeSalaryHistoryCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateEmployeeSalaryHistoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeSalaryHistoryEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            await EmployeeChildHelpers.EnsureEmployeeExistsAsync(request.EmployeeId, _context, cancellationToken);
            EmployeeChildHelpers.ValidateSalaryHistory(request);

            var oldValue = EmployeeMapper.ToSalaryHistoryDto(entity);
            entity.EmployeeId = request.EmployeeId;
            entity.EffectiveDate = request.EffectiveDate;
            entity.OldBasicSalary = request.OldBasicSalary;
            entity.NewBasicSalary = request.NewBasicSalary;
            entity.Allowance = request.Allowance;
            entity.ChangeType = EmployeeChildHelpers.TrimOrNull(request.ChangeType);
            entity.Reason = EmployeeChildHelpers.TrimOrNull(request.Reason);
            entity.DecisionNumber = EmployeeChildHelpers.TrimOrNull(request.DecisionNumber);
            entity.ApprovedBy = EmployeeChildHelpers.TrimOrNull(request.ApprovedBy);
            entity.Note = EmployeeChildHelpers.TrimOrNull(request.Note);
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "EmployeeEntity",
                entity.Id,
                oldValue,
                EmployeeMapper.ToSalaryHistoryDto(entity),
                "Cập nhật lịch sử lương hiệu lực " + entity.EffectiveDate.ToString("yyyy-MM-dd"));

            return true;
        }
    }

    public class DeleteEmployeeSalaryHistoryCommandHandler : IRequestHandler<DeleteEmployeeSalaryHistoryCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeleteEmployeeSalaryHistoryCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteEmployeeSalaryHistoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.EmployeeSalaryHistoryEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "EmployeeEntity",
                entity.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Xóa lịch sử lương hiệu lực " + entity.EffectiveDate.ToString("yyyy-MM-dd"));

            return true;
        }
    }
    #endregion

    internal static class EmployeeChildHelpers
    {
        public static async Task EnsureEmployeeExistsAsync(
            Guid employeeId,
            IApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            var exists = await context.EmployeeEntities
                .AnyAsync(x => x.Id == employeeId, cancellationToken);
            if (!exists)
                throw new InvalidOperationException("Nhân viên không tồn tại.");
        }

        public static void ValidateDependent(EmployeeDependentFields request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new InvalidOperationException("Họ tên người phụ thuộc là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Relationship))
                throw new InvalidOperationException("Mối quan hệ là bắt buộc.");
        }

        public static void ValidateSalaryHistory(EmployeeSalaryHistoryFields request)
        {
            if (request.EffectiveDate == default)
                throw new InvalidOperationException("Ngày hiệu lực là bắt buộc.");
            if (request.NewBasicSalary < 0)
                throw new InvalidOperationException("Lương cơ bản mới không hợp lệ.");
        }

        public static string? TrimOrNull(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
