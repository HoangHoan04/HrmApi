using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.ShiftMaster;
using HrmApi.Application.Features.ShiftMasters.Commands;
using HrmApi.Application.Features.ShiftMasters.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/shift-master")]
    public class ShiftMastersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ShiftMastersController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<ShiftMasterDto>>> GetPaged([FromBody] GetShiftMastersPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<ShiftMasterDto>> GetDetail([FromBody] GetShiftMasterByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy ca làm việc.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateShiftMasterCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateShiftMasterCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy ca làm việc.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateShiftMasterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy ca làm việc.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateShiftMasterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy ca làm việc.");
            return Ok(result);
        }

        [HttpPost("select-box")]
        public async Task<ActionResult<List<ShiftMasterSelectBoxDto>>> GetSelectBox([FromBody] GetShiftMasterSelectBoxQuery query)
            => Ok(await _mediator.Send(query));
    }
}
