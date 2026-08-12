using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.RegisterDayOff;
using HrmApi.Application.Features.DayOffAllocations;
using HrmApi.Application.Features.RegisterDayOffs.Commands;
using HrmApi.Application.Features.RegisterDayOffs.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/register-day-off")]
    public class RegisterDayOffsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RegisterDayOffsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateLeaveView)]
        public async Task<ActionResult<PagedResult<RegisterDayOffDto>>> GetPaged([FromBody] GetRegisterDayOffsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OperateLeaveView)]
        public async Task<ActionResult<RegisterDayOffDto>> GetDetail([FromBody] GetRegisterDayOffByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy đơn nghỉ phép.");
            return Ok(result);
        }

        [HttpPost("preview-days")]
        [RequirePermission(PermissionCodes.OperateLeaveView)]
        public async Task<ActionResult<PreviewLeaveDaysDto>> PreviewDays([FromBody] PreviewLeaveDaysQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OperateLeaveCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateRegisterDayOffCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.OperateLeaveApprove)]
        public async Task<ActionResult<bool>> Approve([FromBody] ApproveRegisterDayOffCommand command)
        {
            try
            {
                command.AsAdmin = true;
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        [RequirePermission(PermissionCodes.OperateLeaveReject)]
        public async Task<ActionResult<bool>> Reject([FromBody] RejectRegisterDayOffCommand command)
        {
            try
            {
                command.AsAdmin = true;
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.OperateLeaveCancel)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelRegisterDayOffCommand command)
        {
            try
            {
                command.AsAdmin = true;
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }

    [ApiController]
    [Authorize]
    [Route("api/v1/day-off-allocation")]
    public class DayOffAllocationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DayOffAllocationsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateLeaveAllocationView)]
        public async Task<ActionResult<PagedResult<DayOffAllocationDto>>> GetPaged([FromBody] GetDayOffAllocationsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("upsert")]
        [RequirePermission(PermissionCodes.OperateLeaveAllocationManage)]
        public async Task<ActionResult<Guid>> Upsert([FromBody] UpsertDayOffAllocationCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
