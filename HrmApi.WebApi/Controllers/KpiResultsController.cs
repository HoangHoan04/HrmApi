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
    [Route("api/v1/kpi-result")]
    public class KpiResultsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public KpiResultsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PerformanceResultView)]
        public async Task<ActionResult<PagedResult<KpiResultDto>>> GetPaged([FromBody] GetKpiResultsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.PerformanceResultView)]
        public async Task<ActionResult<KpiResultDto>> GetDetail([FromBody] GetKpiResultByIdQuery query)
        {
            KpiResultDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy kết quả KPI.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PerformanceResultManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateKpiResultCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.PerformanceResultManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateKpiResultCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy kết quả KPI.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.PerformanceResultManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteKpiResultCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy kết quả KPI.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
