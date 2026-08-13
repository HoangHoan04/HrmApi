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
    [Route("api/v1/notification-template")]
    public class NotificationTemplatesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public NotificationTemplatesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.SystemSettingsView)]
        public async Task<ActionResult<PagedResult<NotificationTemplateDto>>> GetPaged([FromBody] GetNotificationTemplatesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.SystemSettingsView)]
        public async Task<ActionResult<NotificationTemplateDto>> GetDetail([FromBody] GetNotificationTemplateByIdQuery query)
        {
            NotificationTemplateDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy mẫu thông báo.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateNotificationTemplateCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateNotificationTemplateCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy mẫu thông báo.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.SystemSettingsManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteNotificationTemplateCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy mẫu thông báo.");
            return Ok(result);
        }
    }
}
