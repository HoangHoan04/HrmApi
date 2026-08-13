using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Application.Features.Settings;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/system-retention")]
    public class SystemRetentionController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SystemRetentionController(IMediator mediator) => _mediator = mediator;

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.SystemSettingsView)]
        public async Task<ActionResult<SystemRetentionConfigDto>> GetDetail()
            => Ok(await _mediator.Send(new GetSystemRetentionConfigQuery()));

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<SystemRetentionConfigDto>> Update([FromBody] UpdateSystemRetentionConfigCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
