using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
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
    [Route("api/v1/ip-allowlist")]
    public class IpAllowlistController : ControllerBase
    {
        private readonly IMediator _mediator;
        public IpAllowlistController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<PagedResult<IpAllowlistEntryDto>>> GetPaged([FromBody] GetIpAllowlistPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<IpAllowlistEntryDto>> GetDetail([FromBody] GetIpAllowlistByIdQuery query)
        {
            IpAllowlistEntryDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy IP allowlist entry.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<IpAllowlistEntryDto>> Create([FromBody] CreateIpAllowlistCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateIpAllowlistCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy IP allowlist entry.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteIpAllowlistCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy IP allowlist entry.");
            return Ok(result);
        }
    }
}
