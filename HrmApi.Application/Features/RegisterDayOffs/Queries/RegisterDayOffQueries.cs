using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.RegisterDayOff;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.RegisterDayOffs.Queries
{
    public class GetRegisterDayOffsPagedQuery : PagedRequest, IRequest<PagedResult<RegisterDayOffDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Status { get; set; }
        public string? DayOffType { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetRegisterDayOffsPagedQueryHandler : IRequestHandler<GetRegisterDayOffsPagedQuery, PagedResult<RegisterDayOffDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetRegisterDayOffsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<RegisterDayOffDto>> Handle(GetRegisterDayOffsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.RegisterDayOffEntities.AsNoTracking();

            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            else
                query = query.Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(x => x.Status == request.Status.Trim());
            if (!string.IsNullOrWhiteSpace(request.DayOffType))
                query = query.Where(x => x.DayOffType == request.DayOffType.Trim());
            if (request.FromDate.HasValue)
                query = query.Where(x => x.ToDate >= request.FromDate.Value);
            if (request.ToDate.HasValue)
                query = query.Where(x => x.FromDate <= request.ToDate.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            query = query.OrderByDescending(x => x.CreatedAt);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = await MapAsync(entities, cancellationToken);
            return new PagedResult<RegisterDayOffDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private async Task<List<RegisterDayOffDto>> MapAsync(
            List<Domain.Entities.Leave.RegisterDayOffEntity> entities,
            CancellationToken cancellationToken)
        {
            var employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            var configIds = entities.Where(x => x.DayOffConfigId.HasValue).Select(x => x.DayOffConfigId!.Value).Distinct().ToList();
            var approverIds = entities.Where(x => x.ApproverId.HasValue).Select(x => x.ApproverId!.Value).Distinct().ToList();

            var employees = employeeIds.Count == 0
                ? new Dictionary<Guid, (string? Name, string Code)>()
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => employeeIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => (Name: (string?)(x.FullName ?? $"{x.LastName} {x.FirstName}".Trim()), Code: x.Code),
                        cancellationToken);

            var branches = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var configs = configIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => configIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var approvers = approverIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.UserEntities.AsNoTracking()
                    .Where(x => approverIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Username ?? x.Id.ToString(), cancellationToken);

            return entities.Select(x =>
            {
                employees.TryGetValue(x.EmployeeId, out var emp);
                string? branchName = x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out var bn) ? bn : null;
                string? configName = x.DayOffConfigId.HasValue && configs.TryGetValue(x.DayOffConfigId.Value, out var cn) ? cn : null;
                string? approverName = x.ApproverId.HasValue && approvers.TryGetValue(x.ApproverId.Value, out var an) ? an : null;
                return RegisterDayOffMapper.ToDto(x, emp.Name, emp.Code, branchName, configName, approverName);
            }).ToList();
        }
    }

    public class GetRegisterDayOffByIdQuery : IRequest<RegisterDayOffDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetRegisterDayOffByIdQueryHandler : IRequestHandler<GetRegisterDayOffByIdQuery, RegisterDayOffDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetRegisterDayOffByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<RegisterDayOffDto?> Handle(GetRegisterDayOffByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.RegisterDayOffEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return null;

            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == entity.EmployeeId)
                .Select(x => new { x.FullName, x.LastName, x.FirstName, x.Code })
                .FirstOrDefaultAsync(cancellationToken);

            string? branchName = null;
            if (entity.BranchId.HasValue)
            {
                branchName = await _context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == entity.BranchId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? configName = null;
            if (entity.DayOffConfigId.HasValue)
            {
                configName = await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => x.Id == entity.DayOffConfigId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? approverName = null;
            if (entity.ApproverId.HasValue)
            {
                approverName = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == entity.ApproverId)
                    .Select(x => x.Username)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return RegisterDayOffMapper.ToDto(
                entity,
                emp?.FullName ?? $"{emp?.LastName} {emp?.FirstName}".Trim(),
                emp?.Code,
                branchName,
                configName,
                approverName);
        }
    }

    public class GetMyRegisterDayOffsQuery : IRequest<List<RegisterDayOffDto>>
    {
        public string? Status { get; set; }
        public int? Year { get; set; }
    }

    public class GetMyRegisterDayOffsQueryHandler : IRequestHandler<GetMyRegisterDayOffsQuery, List<RegisterDayOffDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyRegisterDayOffsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<RegisterDayOffDto>> Handle(GetMyRegisterDayOffsQuery request, CancellationToken cancellationToken)
        {
            var employeeId = await ResolveEmployeeIdAsync(cancellationToken);

            var query = _context.RegisterDayOffEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(x => x.Status == request.Status.Trim());
            if (request.Year.HasValue)
                query = query.Where(x => x.FromDate.Year == request.Year.Value || x.ToDate.Year == request.Year.Value);

            var entities = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

            var configIds = entities.Where(x => x.DayOffConfigId.HasValue).Select(x => x.DayOffConfigId!.Value).Distinct().ToList();
            var configs = configIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => configIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return entities.Select(x =>
            {
                string? configName = x.DayOffConfigId.HasValue && configs.TryGetValue(x.DayOffConfigId.Value, out var cn) ? cn : null;
                return RegisterDayOffMapper.ToDto(x, null, null, null, configName);
            }).ToList();
        }

        private async Task<Guid> ResolveEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
                return _currentUser.EmployeeId.Value;

            if (_currentUser.UserId.HasValue)
            {
                var empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                    return empId.Value;
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }
    }
}
