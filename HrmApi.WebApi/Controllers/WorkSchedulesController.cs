using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.WorkSchedule;
using HrmApi.Application.Features.WorkSchedules.Commands;
using HrmApi.Application.Features.WorkSchedules.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/work-schedule")]
    public class WorkSchedulesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public WorkSchedulesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<WorkScheduleDto>>> GetPaged([FromBody] GetWorkSchedulesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<WorkScheduleDto>> GetDetail([FromBody] GetWorkScheduleByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy lịch làm việc.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateWorkScheduleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateWorkScheduleCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch làm việc.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateWorkScheduleCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy lịch làm việc.");
            return Ok(result);
        }
    }
}
