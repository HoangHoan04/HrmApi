using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Notification;
using HrmApi.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<NotificationDto>>> GetPaged([FromBody] GetNotificationsPagedQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var result = await _mediator.Send(new GetUnreadNotificationCountQuery());
            return Ok(result);
        }

        [HttpPost("mark-read")]
        public async Task<ActionResult<bool>> MarkRead([FromBody] MarkNotificationReadCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("mark-all-read")]
        public async Task<ActionResult<int>> MarkAllRead()
        {
            var result = await _mediator.Send(new MarkAllNotificationsReadCommand());
            return Ok(result);
        }

        [HttpPost("delete")]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteNotificationCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("broadcast")]
        public async Task<ActionResult<int>> SendBroadcast([FromBody] SendBroadcastNotificationCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("settings")]
        public async Task<ActionResult<NotificationSettingDto>> GetSettings()
        {
            var result = await _mediator.Send(new GetNotificationSettingsQuery());
            return Ok(result);
        }

        [HttpPost("settings")]
        public async Task<ActionResult<bool>> UpdateSettings([FromBody] UpdateNotificationSettingsCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
