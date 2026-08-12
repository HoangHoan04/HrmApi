using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Role;
using HrmApi.Application.Features.UserRoles;
using HrmApi.Application.Features.Users;
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

    [ApiController]
    [Authorize]
    [Route("api/v1/user")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.UserView)]
        public async Task<ActionResult<PagedResult<UserListItemDto>>> Pagination([FromBody] GetUsersPagedQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.UserView)]
        public async Task<ActionResult<UserDetailDto>> Detail([FromBody] GetUserByIdQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                return result == null ? NotFound("Không tìm thấy tài khoản.") : Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.UserCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateUserCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.UserUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateUserCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reset-password")]
        [RequirePermission(PermissionCodes.UserResetPassword)]
        public async Task<ActionResult<bool>> ResetPassword([FromBody] ResetUserPasswordCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.UserDelete)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteUserCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
