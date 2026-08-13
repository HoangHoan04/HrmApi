using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
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
    [Route("api/v1/kpi-goal")]
    public class KpiGoalsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public KpiGoalsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PerformanceGoalView)]
        public async Task<ActionResult<PagedResult<KpiGoalDto>>> GetPaged([FromBody] GetKpiGoalsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.PerformanceGoalView)]
        public async Task<ActionResult<KpiGoalDto>> GetDetail([FromBody] GetKpiGoalByIdQuery query)
        {
            KpiGoalDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy mục tiêu KPI.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PerformanceGoalManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateKpiGoalCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.PerformanceGoalManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateKpiGoalCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy mục tiêu KPI.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.PerformanceGoalManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteKpiGoalCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy mục tiêu KPI.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
