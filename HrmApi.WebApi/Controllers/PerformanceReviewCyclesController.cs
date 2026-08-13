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
    [Route("api/v1/performance-cycle")]
    public class PerformanceReviewCyclesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PerformanceReviewCyclesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PerformanceCycleView)]
        public async Task<ActionResult<PagedResult<PerformanceReviewCycleDto>>> GetPaged([FromBody] GetReviewCyclesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.PerformanceCycleView)]
        public async Task<ActionResult<PerformanceReviewCycleDto>> GetDetail([FromBody] GetReviewCycleByIdQuery query)
        {
            PerformanceReviewCycleDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy chu kỳ đánh giá.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PerformanceCycleCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateReviewCycleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.PerformanceCycleUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateReviewCycleCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy chu kỳ đánh giá.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.PerformanceCycleUpdate)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteReviewCycleCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy chu kỳ đánh giá.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
