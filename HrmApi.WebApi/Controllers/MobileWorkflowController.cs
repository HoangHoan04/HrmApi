using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Workflow;
using HrmApi.Application.Features.Workflow;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [RequirePermission(PermissionCodes.MobileAccess)]
    [Route("api/v1/mobile/workflow")]
    public class MobileWorkflowController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileWorkflowController(IMediator mediator) => _mediator = mediator;

        [HttpPost("inbox")]
        public async Task<ActionResult<List<WorkflowInboxItemDto>>> Inbox([FromBody] GetWorkflowInboxQuery? query)
        {
            try
            {
                var q = query ?? new GetWorkflowInboxQuery();
                q.EmployeeId = null; // force current employee
                return Ok(await _mediator.Send(q));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("advance")]
        public async Task<ActionResult<bool>> Advance([FromBody] AdvanceWorkflowTaskCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        public async Task<ActionResult<bool>> Reject([FromBody] RejectWorkflowTaskCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
