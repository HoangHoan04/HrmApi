using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.DTOs.Branch;
using HrmApi.Application.DTOs.Department;
using HrmApi.Application.DTOs.Part;
using HrmApi.Application.DTOs.PartMaster;
using HrmApi.Application.DTOs.PositionMaster;
using HrmApi.Application.Features.Branches.Queries;
using HrmApi.Application.Features.Departments.Queries;
using HrmApi.Application.Features.PartMasters.Queries;
using HrmApi.Application.Features.Parts.Queries;
using HrmApi.Application.Features.PositionMasters.Queries;
using MediatR;

namespace HrmApi.Application.Features.Organization.Queries
{
    /// <summary>
    /// Load chi nhánh theo công ty (bắt buộc companyId).
    /// </summary>
    public class GetBranchesByCompanyQuery : IRequest<List<BranchSelectBoxDto>>
    {
        public Guid CompanyId { get; set; }
        public Guid? ExcludeId { get; set; }
    }

    public class GetBranchesByCompanyQueryHandler : IRequestHandler<GetBranchesByCompanyQuery, List<BranchSelectBoxDto>>
    {
        private readonly IMediator _mediator;

        public GetBranchesByCompanyQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<List<BranchSelectBoxDto>> Handle(GetBranchesByCompanyQuery request, CancellationToken cancellationToken)
        {
            if (request.CompanyId == Guid.Empty)
                throw new InvalidOperationException("Id công ty là bắt buộc.");

            return _mediator.Send(new GetBranchSelectBoxQuery
            {
                CompanyId = request.CompanyId,
                ExcludeId = request.ExcludeId
            }, cancellationToken);
        }
    }

    /// <summary>
    /// Load phòng ban theo chi nhánh (bắt buộc branchId).
    /// </summary>
    public class GetDepartmentsByBranchQuery : IRequest<List<DepartmentSelectBoxDto>>
    {
        public Guid BranchId { get; set; }
        public Guid? ExcludeId { get; set; }
    }

    public class GetDepartmentsByBranchQueryHandler : IRequestHandler<GetDepartmentsByBranchQuery, List<DepartmentSelectBoxDto>>
    {
        private readonly IMediator _mediator;

        public GetDepartmentsByBranchQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<List<DepartmentSelectBoxDto>> Handle(GetDepartmentsByBranchQuery request, CancellationToken cancellationToken)
        {
            if (request.BranchId == Guid.Empty)
                throw new InvalidOperationException("Id chi nhánh là bắt buộc.");

            return _mediator.Send(new GetDepartmentSelectBoxQuery
            {
                BranchId = request.BranchId,
                ExcludeId = request.ExcludeId
            }, cancellationToken);
        }
    }

    /// <summary>
    /// Load bộ phận (Part) theo phòng ban (bắt buộc departmentId).
    /// </summary>
    public class GetPartsByDepartmentQuery : IRequest<List<PartSelectBoxDto>>
    {
        public Guid DepartmentId { get; set; }
        public Guid? ExcludeId { get; set; }
    }

    public class GetPartsByDepartmentQueryHandler : IRequestHandler<GetPartsByDepartmentQuery, List<PartSelectBoxDto>>
    {
        private readonly IMediator _mediator;

        public GetPartsByDepartmentQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<List<PartSelectBoxDto>> Handle(GetPartsByDepartmentQuery request, CancellationToken cancellationToken)
        {
            if (request.DepartmentId == Guid.Empty)
                throw new InvalidOperationException("Id phòng ban là bắt buộc.");

            return _mediator.Send(new GetPartSelectBoxQuery
            {
                DepartmentId = request.DepartmentId,
                ExcludeId = request.ExcludeId
            }, cancellationToken);
        }
    }

    /// <summary>
    /// Load mẫu bộ phận theo công ty/chi nhánh.
    /// </summary>
    public class GetPartMastersByScopeQuery : IRequest<List<PartMasterSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetPartMastersByScopeQueryHandler : IRequestHandler<GetPartMastersByScopeQuery, List<PartMasterSelectBoxDto>>
    {
        private readonly IMediator _mediator;

        public GetPartMastersByScopeQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<List<PartMasterSelectBoxDto>> Handle(GetPartMastersByScopeQuery request, CancellationToken cancellationToken)
        {
            return _mediator.Send(new GetPartMasterSelectBoxQuery
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId
            }, cancellationToken);
        }
    }

    /// <summary>
    /// Load mẫu chức danh theo công ty/chi nhánh.
    /// </summary>
    public class GetPositionMastersByScopeQuery : IRequest<List<PositionMasterSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetPositionMastersByScopeQueryHandler : IRequestHandler<GetPositionMastersByScopeQuery, List<PositionMasterSelectBoxDto>>
    {
        private readonly IMediator _mediator;

        public GetPositionMastersByScopeQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<List<PositionMasterSelectBoxDto>> Handle(GetPositionMastersByScopeQuery request, CancellationToken cancellationToken)
        {
            return _mediator.Send(new GetPositionMasterSelectBoxQuery
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId
            }, cancellationToken);
        }
    }
}
