using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Role;
using HrmApi.Application.Features.Roles;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/role")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.RoleView)]
        public async Task<ActionResult<PagedResult<RoleListItemDto>>> Pagination([FromBody] GetRolesPagedQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.RoleView)]
        public async Task<ActionResult<System.Collections.Generic.List<RoleSelectBoxDto>>> SelectBox([FromBody] GetRoleSelectBoxQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.RoleView)]
        public async Task<ActionResult<RoleDetailDto>> Detail([FromBody] GetRoleByIdQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                return result == null ? NotFound("Không tìm thấy vai trò.") : Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.RoleCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateRoleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.RoleUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateRoleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.RoleDelete)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteRoleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("set-permissions")]
        [RequirePermission(PermissionCodes.RoleManage)]
        public async Task<ActionResult<bool>> SetPermissions([FromBody] SetRolePermissionsCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
