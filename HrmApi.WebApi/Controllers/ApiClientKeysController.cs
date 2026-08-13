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
    [Route("api/v1/api-client-key")]
    public class ApiClientKeysController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ApiClientKeysController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.SystemSettingsView)]
        public async Task<ActionResult<PagedResult<ApiClientKeyDto>>> GetPaged([FromBody] GetApiClientKeysPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.SystemSettingsView)]
        public async Task<ActionResult<ApiClientKeyDto>> GetDetail([FromBody] GetApiClientKeyByIdQuery query)
        {
            ApiClientKeyDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy API key.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<ApiClientKeyCreateResultDto>> Create([FromBody] CreateApiClientKeyCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateApiClientKeyCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy API key.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteApiClientKeyCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy API key.");
            return Ok(result);
        }
    }
}
