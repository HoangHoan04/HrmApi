using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.DTOs.Performance;
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
    [Route("api/v1/mobile/performance")]
    public class MobilePerformanceController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobilePerformanceController(IMediator mediator) => _mediator = mediator;

        [HttpPost("my-goals")]
        public async Task<ActionResult<List<KpiGoalDto>>> MyGoals([FromBody] GetMyKpiGoalsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyKpiGoalsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("my-results")]
        public async Task<ActionResult<List<KpiResultDto>>> MyResults([FromBody] GetMyKpiResultsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyKpiResultsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("my-360")]
        public async Task<ActionResult<List<Performance360ReviewDto>>> My360([FromBody] GetMyPerformance360Query? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyPerformance360Query())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("upsert-360")]
        public async Task<ActionResult<Guid>> Upsert360([FromBody] UpsertMyPerformance360Command command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
