using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Discipline;
using HrmApi.Application.Features.Discipline;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/violation")]
    public class ViolationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ViolationsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.DisciplineViolationView)]
        public async Task<ActionResult<PagedResult<ViolationDto>>> GetPaged([FromBody] GetViolationsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.DisciplineViolationView)]
        public async Task<ActionResult<ViolationDto>> GetDetail([FromBody] GetViolationByIdQuery query)
        {
            ViolationDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy biên bản vi phạm.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.DisciplineViolationCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateViolationCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.DisciplineViolationUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateViolationCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy biên bản vi phạm.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.DisciplineViolationDelete)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteViolationCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy biên bản vi phạm.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("confirm")]
        [RequirePermission(PermissionCodes.DisciplineViolationApprove)]
        public async Task<ActionResult<bool>> Confirm([FromBody] ConfirmViolationCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy biên bản vi phạm.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.DisciplineViolationApprove)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelViolationCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy biên bản vi phạm.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
