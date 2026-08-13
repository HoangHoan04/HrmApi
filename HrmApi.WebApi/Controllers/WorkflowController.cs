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
    [Route("api/v1/workflow")]
    public class WorkflowController : ControllerBase
    {
        private readonly IMediator _mediator;
        public WorkflowController(IMediator mediator) => _mediator = mediator;

        [HttpPost("inbox")]
        [RequirePermission(PermissionCodes.WorkflowInbox)]
        public async Task<ActionResult<List<WorkflowInboxItemDto>>> Inbox(
            [FromBody] GetWorkflowInboxQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetWorkflowInboxQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("dashboard-summary")]
        [RequirePermission(PermissionCodes.WorkflowView)]
        public async Task<ActionResult<WorkflowDashboardSummaryDto>> DashboardSummary(
            [FromBody] GetWorkflowDashboardSummaryQuery? query)
            => Ok(await _mediator.Send(query ?? new GetWorkflowDashboardSummaryQuery()));

        [HttpPost("advance")]
        [RequirePermission(PermissionCodes.WorkflowInbox)]
        public async Task<ActionResult<bool>> Advance([FromBody] AdvanceWorkflowTaskCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        [RequirePermission(PermissionCodes.WorkflowInbox)]
        public async Task<ActionResult<bool>> Reject([FromBody] RejectWorkflowTaskCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
