using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Contract;
using HrmApi.Application.DTOs.Employee;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.DTOs.Organization;
using HrmApi.Application.Features.Mobile;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [RequirePermission(PermissionCodes.MobileAccess)]
    [Route("api/v1/mobile/profile")]
    public class MobileProfileController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileProfileController(IMediator mediator) => _mediator = mediator;

        [HttpPost("contracts")]
        public async Task<ActionResult<List<ContractDto>>> Contracts([FromBody] GetMyContractsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyContractsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("files")]
        public async Task<ActionResult<List<EmployeeFileDto>>> Files([FromBody] GetMyEmployeeFilesQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyEmployeeFilesQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("directory")]
        public async Task<ActionResult<PagedResult<MobileDirectoryEmployeeDto>>> Directory([FromBody] GetMobileDirectoryQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("org-chart")]
        public async Task<ActionResult<OrgChartNodeDto?>> OrgChart([FromBody] GetMyOrgChartQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyOrgChartQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
