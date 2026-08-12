using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.AttendanceComplaint;
using HrmApi.Application.Features.AttendanceComplaints;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/attendance-complaint")]
    public class AttendanceComplaintsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AttendanceComplaintsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateAttendanceComplaintView)]
        public async Task<ActionResult<PagedResult<AttendanceComplaintDto>>> Pagination(
            [FromBody] GetAttendanceComplaintsPagedQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OperateAttendanceComplaintView)]
        public async Task<ActionResult<AttendanceComplaintDto>> Detail(
            [FromBody] GetAttendanceComplaintByIdQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                return result == null ? NotFound() : Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OperateAttendanceComplaintCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateAttendanceComplaintCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.OperateAttendanceComplaintReview)]
        public async Task<ActionResult<bool>> Approve([FromBody] ReviewAttendanceComplaintCommand command)
        {
            try
            {
                command.Approve = true;
                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        [RequirePermission(PermissionCodes.OperateAttendanceComplaintReview)]
        public async Task<ActionResult<bool>> Reject([FromBody] ReviewAttendanceComplaintCommand command)
        {
            try
            {
                command.Approve = false;
                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.OperateAttendanceComplaintManage)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelAttendanceComplaintCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }

    [ApiController]
    [Authorize]
    [RequirePermission(PermissionCodes.MobileAccess)]
    [Route("api/v1/mobile/attendance-complaint")]
    public class MobileAttendanceComplaintController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileAttendanceComplaintController(IMediator mediator) => _mediator = mediator;

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateAttendanceComplaintCommand command)
        {
            try
            {
                command.EmployeeId = null; // always self
                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("my-list")]
        public async Task<ActionResult<List<AttendanceComplaintDto>>> MyList(
            [FromBody] GetMyAttendanceComplaintsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyAttendanceComplaintsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelAttendanceComplaintCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
