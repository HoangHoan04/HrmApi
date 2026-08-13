using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
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
    [Route("api/v1/workflow-definition")]
    public class WorkflowDefinitionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public WorkflowDefinitionsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.WorkflowView)]
        public async Task<ActionResult<PagedResult<WorkflowDefinitionDto>>> GetPaged(
            [FromBody] GetWorkflowDefinitionsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.WorkflowView)]
        public async Task<ActionResult<WorkflowDefinitionDto>> GetDetail(
            [FromBody] GetWorkflowDefinitionByIdQuery query)
        {
            WorkflowDefinitionDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy workflow definition.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.WorkflowManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateWorkflowDefinitionCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.WorkflowManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateWorkflowDefinitionCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy workflow definition.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.WorkflowManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteWorkflowDefinitionCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy workflow definition.");
            return Ok(result);
        }

        [HttpPost("set-steps")]
        [RequirePermission(PermissionCodes.WorkflowManage)]
        public async Task<ActionResult<bool>> SetSteps([FromBody] SetWorkflowStepsCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy workflow definition.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
