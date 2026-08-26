using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Role;
using HrmApi.Application.Features.UserRoles;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/user-role")]
    public class UserRolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserRolesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("by-user")]
        [RequirePermission(PermissionCodes.UserView)]
        public async Task<ActionResult<List<UserRoleItemDto>>> ByUser([FromBody] GetUserRolesByUserQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("by-employee")]
        [RequirePermission(PermissionCodes.UserView)]
        public async Task<ActionResult<List<UserRoleItemDto>>> ByEmployee([FromBody] GetUserRolesByEmployeeQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("set")]
        [RequirePermission(PermissionCodes.UserUpdate)]
        public async Task<ActionResult<bool>> Set([FromBody] SetUserRolesCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("set-by-employee")]
        [RequirePermission(PermissionCodes.UserUpdate)]
        public async Task<ActionResult<bool>> SetByEmployee([FromBody] SetUserRolesByEmployeeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
