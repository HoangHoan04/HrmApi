using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Mobile;
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
    [Route("api/v1/mobile/dashboard")]
    public class MobileDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileDashboardController(IMediator mediator) => _mediator = mediator;

        [HttpPost("manager-summary")]
        public async Task<ActionResult<MobileManagerSummaryDto>> ManagerSummary([FromBody] GetMobileManagerSummaryQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMobileManagerSummaryQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
