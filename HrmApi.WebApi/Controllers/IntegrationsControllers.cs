using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Application.Features.Integrations;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/sms-gateway-config")]
    public class SmsGatewayConfigsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SmsGatewayConfigsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.IntegrationsView)]
        public async Task<ActionResult<PagedResult<SmsGatewayConfigDto>>> GetPaged([FromBody] GetSmsGatewayConfigsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.IntegrationsView)]
        public async Task<ActionResult<SmsGatewayConfigDto>> GetDetail([FromBody] GetSmsGatewayConfigByIdQuery query)
        {
            SmsGatewayConfigDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy cấu hình SMS.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.IntegrationsManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSmsGatewayConfigCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.IntegrationsManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateSmsGatewayConfigCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy cấu hình SMS.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.IntegrationsManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteSmsGatewayConfigCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình SMS.");
            return Ok(result);
        }

        [HttpPost("send-test")]
        [RequirePermission(PermissionCodes.IntegrationsManage)]
        public async Task<ActionResult<string>> SendTest([FromBody] SendSmsTestCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }

    [ApiController]
    [Authorize]
    [Route("api/v1/zalo-oa-config")]
    public class ZaloOaConfigsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ZaloOaConfigsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.IntegrationsView)]
        public async Task<ActionResult<PagedResult<ZaloOaConfigDto>>> GetPaged([FromBody] GetZaloOaConfigsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.IntegrationsView)]
        public async Task<ActionResult<ZaloOaConfigDto>> GetDetail([FromBody] GetZaloOaConfigByIdQuery query)
        {
            ZaloOaConfigDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy cấu hình Zalo OA.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.IntegrationsManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateZaloOaConfigCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.IntegrationsManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateZaloOaConfigCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy cấu hình Zalo OA.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.IntegrationsManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteZaloOaConfigCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình Zalo OA.");
            return Ok(result);
        }
    }

    [ApiController]
    [Route("api/v1/integrations")]
    public class IntegrationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public IntegrationsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("status")]
        [Authorize]
        [RequirePermission(PermissionCodes.IntegrationsView)]
        public async Task<ActionResult<IntegrationStatusResultDto>> GetStatus()
            => Ok(await _mediator.Send(new GetIntegrationStatusQuery()));

        [HttpPost("status")]
        [Authorize]
        [RequirePermission(PermissionCodes.IntegrationsView)]
        public async Task<ActionResult<IntegrationStatusResultDto>> RefreshStatus()
            => Ok(await _mediator.Send(new RefreshIntegrationStatusCommand()));

        [HttpPost("zalo/webhook")]
        [AllowAnonymous]
        public IActionResult ZaloWebhook([FromBody] object? payload)
            => Ok(new { ok = true, receivedAt = DateTime.UtcNow });

        [HttpPost("zalo/send-test")]
        [Authorize]
        [RequirePermission(PermissionCodes.IntegrationsManage)]
        public async Task<ActionResult<string>> ZaloSendTest([FromBody] SendZaloTestCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
