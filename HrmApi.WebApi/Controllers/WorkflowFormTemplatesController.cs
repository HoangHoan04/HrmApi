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
    [Route("api/v1/workflow-form-template")]
    public class WorkflowFormTemplatesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public WorkflowFormTemplatesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.WorkflowView)]
        public async Task<ActionResult<PagedResult<WorkflowFormTemplateDto>>> GetPaged(
            [FromBody] GetWorkflowFormTemplatesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.WorkflowView)]
        public async Task<ActionResult<WorkflowFormTemplateDto>> GetDetail(
            [FromBody] GetWorkflowFormTemplateByIdQuery query)
        {
            WorkflowFormTemplateDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy form template.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.WorkflowManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateWorkflowFormTemplateCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.WorkflowManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateWorkflowFormTemplateCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy form template.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.WorkflowManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteWorkflowFormTemplateCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy form template.");
            return Ok(result);
        }
    }
}
