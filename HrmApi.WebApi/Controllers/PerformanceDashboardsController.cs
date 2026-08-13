using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Performance;
using HrmApi.Application.Features.Performance;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/performance-dashboard")]
    public class PerformanceDashboardsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PerformanceDashboardsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("summary")]
        [RequirePermission(PermissionCodes.PerformanceView)]
        public async Task<ActionResult<PerformanceDashboardDto>> GetSummary([FromBody] GetPerformanceDashboardQuery query)
            => Ok(await _mediator.Send(query));
    }
}
