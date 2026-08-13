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
    [Route("api/v1/legal-rate-config")]
    public class LegalRateConfigsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public LegalRateConfigsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.SystemSettingsView)]
        public async Task<ActionResult<PagedResult<LegalRateConfigDto>>> GetPaged([FromBody] GetLegalRateConfigsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.SystemSettingsView)]
        public async Task<ActionResult<LegalRateConfigDto>> GetDetail([FromBody] GetLegalRateConfigByIdQuery query)
        {
            LegalRateConfigDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy cấu hình tỷ lệ.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateLegalRateConfigCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateLegalRateConfigCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy cấu hình tỷ lệ.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteLegalRateConfigCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình tỷ lệ.");
            return Ok(result);
        }
    }
}
