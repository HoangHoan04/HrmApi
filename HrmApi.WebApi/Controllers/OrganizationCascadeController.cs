using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.DTOs.Branch;
using HrmApi.Application.DTOs.Department;
using HrmApi.Application.DTOs.Part;
using HrmApi.Application.DTOs.PartMaster;
using HrmApi.Application.DTOs.PositionMaster;
using HrmApi.Application.Features.Organization.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API load dữ liệu con theo entity cha (cascade dropdown).
    /// </summary>
    [ApiController]
    [Route("api/v1/organization")]
    public class OrganizationCascadeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationCascadeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Load chi nhánh theo id công ty.</summary>
        [HttpPost("branches-by-company")]
        public async Task<ActionResult<List<BranchSelectBoxDto>>> GetBranchesByCompany(
            [FromBody] GetBranchesByCompanyQuery query)
        {
            try
            {
                return Ok(await _mediator.Send(query));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Load phòng ban theo id chi nhánh.</summary>
        [HttpPost("departments-by-branch")]
        public async Task<ActionResult<List<DepartmentSelectBoxDto>>> GetDepartmentsByBranch(
            [FromBody] GetDepartmentsByBranchQuery query)
        {
            try
            {
                return Ok(await _mediator.Send(query));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Load bộ phận theo id phòng ban.</summary>
        [HttpPost("parts-by-department")]
        public async Task<ActionResult<List<PartSelectBoxDto>>> GetPartsByDepartment(
            [FromBody] GetPartsByDepartmentQuery query)
        {
            try
            {
                return Ok(await _mediator.Send(query));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Load mẫu bộ phận theo công ty/chi nhánh.</summary>
        [HttpPost("part-masters-by-scope")]
        public async Task<ActionResult<List<PartMasterSelectBoxDto>>> GetPartMastersByScope(
            [FromBody] GetPartMastersByScopeQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        /// <summary>Load mẫu chức danh theo công ty/chi nhánh.</summary>
        [HttpPost("position-masters-by-scope")]
        public async Task<ActionResult<List<PositionMasterSelectBoxDto>>> GetPositionMastersByScope(
            [FromBody] GetPositionMastersByScopeQuery query)
        {
            return Ok(await _mediator.Send(query));
        }
    }
}
