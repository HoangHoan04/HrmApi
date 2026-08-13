using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.Common.Services;
using HrmApi.Application.DTOs.Employee;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Employees.Queries
{
    #region Paged Query
    public class GetEmployeesPagedQuery : PagedRequest, IRequest<PagedResult<EmployeeDto>>
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetEmployeesPagedQueryHandler : IRequestHandler<GetEmployeesPagedQuery, PagedResult<EmployeeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetEmployeesPagedQueryHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<PagedResult<EmployeeDto>> Handle(GetEmployeesPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.EmployeeEntities.AsNoTracking();
            query = await query.ApplyEmployeeDataScopeAsync(
                _dataScope, PermissionCodes.HrEmployeeView, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                var fullName = request.FullName.Trim().ToLower();
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.ToLower().Contains(fullName))
                    || x.FirstName.ToLower().Contains(fullName)
                    || x.LastName.ToLower().Contains(fullName));
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var phone = request.Phone.Trim().ToLower();
                query = query.Where(x => x.Phone.ToLower().Contains(phone));
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email.Trim().ToLower();
                query = query.Where(x => x.Email.ToLower().Contains(email));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var status = request.Status.Trim().ToLower();
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status);
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(search)
                    || (x.FullName != null && x.FullName.ToLower().Contains(search))
                    || x.FirstName.ToLower().Contains(search)
                    || x.LastName.ToLower().Contains(search)
                    || x.Phone.ToLower().Contains(search)
                    || x.Email.ToLower().Contains(search)
                    || x.IdentityCard.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, request);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = entities.Select(x => EmployeeMapper.ToDto(x)).ToList();

            return new PagedResult<EmployeeDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private static IQueryable<EmployeeEntity> ApplySorting(
            IQueryable<EmployeeEntity> query,
            GetEmployeesPagedQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SortField))
            {
                var isDesc = request.SortOrder?.ToLower() == "desc";
                return request.SortField.ToLower() switch
                {
                    "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
                    "fullname" or "fullname" => isDesc
                        ? query.OrderByDescending(x => x.FullName)
                        : query.OrderBy(x => x.FullName),
                    "phone" => isDesc ? query.OrderByDescending(x => x.Phone) : query.OrderBy(x => x.Phone),
                    "email" => isDesc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
                    "joindate" => isDesc
                        ? query.OrderByDescending(x => x.JoinDate)
                        : query.OrderBy(x => x.JoinDate),
                    "status" => isDesc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status),
                    "isdeleted" => isDesc
                        ? query.OrderByDescending(x => x.IsDeleted)
                        : query.OrderBy(x => x.IsDeleted),
                    "createdat" => isDesc
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt),
                    _ => query.OrderByDescending(x => x.CreatedAt)
                };
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
    #endregion

    #region Detail Query
    public class GetEmployeeByIdQuery : IRequest<EmployeeDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetEmployeeByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeDto?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            var employee = await _context.EmployeeEntities
                .AsNoTracking()
                .Include(x => x.DirectManager)
                .Include(x => x.Dependents)
                .Include(x => x.Educations)
                .Include(x => x.Certificates)
                .Include(x => x.Files)
                .Include(x => x.SalaryHistories)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            return employee == null ? null : EmployeeMapper.ToDto(employee, includeChildren: true);
        }
    }
    #endregion

    #region Select Box Query
    public class GetEmployeeSelectBoxQuery : IRequest<List<EmployeeSelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetEmployeeSelectBoxQueryHandler : IRequestHandler<GetEmployeeSelectBoxQuery, List<EmployeeSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetEmployeeSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeSelectBoxDto>> Handle(GetEmployeeSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.EmployeeEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.ExcludeId.HasValue && request.ExcludeId != Guid.Empty)
            {
                query = query.Where(x => x.Id != request.ExcludeId.Value);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            return await query
                .OrderBy(x => x.FullName)
                .Select(x => new EmployeeSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.FullName ?? (x.FirstName + " " + x.LastName)
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion
}
