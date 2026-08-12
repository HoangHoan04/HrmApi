using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
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
    [RequirePermission(PermissionCodes.MobileAccess)]
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
                command.AsAdmin = false;
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("balance")]
        public async Task<ActionResult<MobileLeaveBalanceDto>> Balance([FromBody] GetMyLeaveBalanceQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyLeaveBalanceQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("preview-days")]
        public async Task<ActionResult<PreviewLeaveDaysDto>> PreviewDays([FromBody] PreviewLeaveDaysQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("pending-approvals")]
        public async Task<ActionResult<List<RegisterDayOffDto>>> PendingApprovals([FromBody] GetPendingApprovalsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetPendingApprovalsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        public async Task<ActionResult<bool>> Approve([FromBody] ApproveRegisterDayOffCommand command)
        {
            try
            {
                command.AsAdmin = false;
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        public async Task<ActionResult<bool>> Reject([FromBody] RejectRegisterDayOffCommand command)
        {
            try
            {
                command.AsAdmin = false;
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
