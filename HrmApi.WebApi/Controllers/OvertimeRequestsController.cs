using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.OvertimeRequest;
using HrmApi.Application.Features.OvertimeRequests;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/overtime-request")]
    public class OvertimeRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OvertimeRequestsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateOvertimeView)]
        public async Task<ActionResult<PagedResult<OvertimeRequestDto>>> Pagination(
            [FromBody] GetOvertimeRequestsPagedQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OperateOvertimeView)]
        public async Task<ActionResult<OvertimeRequestDto>> Detail(
            [FromBody] GetOvertimeRequestByIdQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                return result == null ? NotFound() : Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OperateOvertimeCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateOvertimeRequestCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("submit")]
        [RequirePermission(PermissionCodes.OperateOvertimeCreate)]
        public async Task<ActionResult<bool>> Submit([FromBody] SubmitOvertimeRequestCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.OperateOvertimeApprove)]
        public async Task<ActionResult<bool>> Approve([FromBody] ReviewOvertimeRequestCommand command)
        {
            try
            {
                command.Approve = true;
                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        [RequirePermission(PermissionCodes.OperateOvertimeApprove)]
        public async Task<ActionResult<bool>> Reject([FromBody] ReviewOvertimeRequestCommand command)
        {
            try
            {
                command.Approve = false;
                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("bulk-approve")]
        [RequirePermission(PermissionCodes.OperateOvertimeApprove)]
        public async Task<ActionResult<int>> BulkApprove([FromBody] BulkApproveOvertimeRequestCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.OperateOvertimeManage)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelOvertimeRequestCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
