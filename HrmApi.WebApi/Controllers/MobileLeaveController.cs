using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.DTOs.RegisterDayOff;
using HrmApi.Application.Features.RegisterDayOffs.Commands;
using HrmApi.Application.Features.RegisterDayOffs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/mobile/leave")]
    public class MobileLeaveController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileLeaveController(IMediator mediator) => _mediator = mediator;

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateRegisterDayOffCommand command)
        {
            try
            {
                // Mobile: force employee from JWT (ignore body EmployeeId)
                command.EmployeeId = null;
                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("my-list")]
        public async Task<ActionResult<List<RegisterDayOffDto>>> MyList([FromBody] GetMyRegisterDayOffsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyRegisterDayOffsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelRegisterDayOffCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
