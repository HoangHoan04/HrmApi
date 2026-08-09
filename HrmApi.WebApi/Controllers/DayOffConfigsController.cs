using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.DayOffConfig;
using HrmApi.Application.Features.DayOffConfigs.Commands;
using HrmApi.Application.Features.DayOffConfigs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/day-off-config")]
    public class DayOffConfigsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DayOffConfigsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<DayOffConfigDto>>> GetPaged([FromBody] GetDayOffConfigsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<DayOffConfigDto>> GetDetail([FromBody] GetDayOffConfigByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy cấu hình nghỉ phép.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateDayOffConfigCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateDayOffConfigCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy cấu hình nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateDayOffConfigCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình nghỉ phép.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateDayOffConfigCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình nghỉ phép.");
            return Ok(result);
        }

        [HttpPost("select-box")]
        public async Task<ActionResult<List<DayOffConfigSelectBoxDto>>> GetSelectBox([FromBody] GetDayOffConfigSelectBoxQuery query)
            => Ok(await _mediator.Send(query));
    }
}
