using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Application.Features.Timekeepings.Commands;
using HrmApi.Application.Features.Timekeepings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/timekeeping")]
    public class TimekeepingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TimekeepingsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<TimekeepingDto>>> GetPaged([FromBody] GetTimekeepingsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<TimekeepingDto>> GetDetail([FromBody] GetTimekeepingByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy bản ghi chấm công.");
            return Ok(result);
        }

        [HttpPost("adjust")]
        public async Task<ActionResult<bool>> Adjust([FromBody] ManualAdjustTimekeepingCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy bản ghi chấm công.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("summarize")]
        public async Task<ActionResult<int>> Summarize([FromBody] SummarizeMonthTimekeepingCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("summary/pagination")]
        public async Task<ActionResult<PagedResult<TimekeepingSummaryDto>>> GetSummaryPaged([FromBody] GetTimekeepingSummariesPagedQuery query)
            => Ok(await _mediator.Send(query));
    }
}
