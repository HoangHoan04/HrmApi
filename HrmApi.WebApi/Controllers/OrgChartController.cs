using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Organization;
using HrmApi.Application.Features.Organization;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/org-chart")]
    public class OrgChartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrgChartController(IMediator mediator) => _mediator = mediator;

        [HttpPost("tree")]
        [RequirePermission(PermissionCodes.OrgView)]
        public async Task<ActionResult<OrgChartNodeDto>> Tree([FromBody] GetOrgChartTreeQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                if (result == null) return NotFound("Không tìm thấy công ty.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reparent")]
        [RequirePermission(PermissionCodes.OrgManage)]
        public async Task<ActionResult<bool>> Reparent([FromBody] ReparentOrgChartNodeCommand command)
        {
            try
            {
                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
