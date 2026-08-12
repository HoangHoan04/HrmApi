using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Permission;
using HrmApi.Application.Features.Permissions;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/permission")]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PermissionsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("list")]
        [RequirePermission(PermissionCodes.RoleView)]
        public async Task<ActionResult<System.Collections.Generic.List<PermissionDto>>> List(
            [FromBody] ListPermissionsQuery? query)
        {
            return Ok(await _mediator.Send(query ?? new ListPermissionsQuery()));
        }

        [HttpPost("tree")]
        [RequirePermission(PermissionCodes.RoleView)]
        public async Task<ActionResult<System.Collections.Generic.List<PermissionModuleTreeDto>>> Tree(
            [FromBody] GetPermissionTreeQuery? query)
        {
            return Ok(await _mediator.Send(query ?? new GetPermissionTreeQuery()));
        }
    }
}
