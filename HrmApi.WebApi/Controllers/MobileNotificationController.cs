using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
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
    [Route("api/v1/mobile/notifications")]
    public class MobileNotificationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUser;

        public MobileNotificationController(
            IMediator mediator,
            INotificationService notificationService,
            ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _notificationService = notificationService;
            _currentUser = currentUser;
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

        [HttpPost("register-device-token")]
        public async Task<ActionResult<bool>> RegisterDeviceToken([FromBody] RegisterDeviceTokenDto dto)
        {
            var userId = _currentUser.UserId ?? Guid.Empty;
            if (userId == Guid.Empty) return Unauthorized();

            await _notificationService.RegisterDeviceTokenAsync(userId, _currentUser.EmployeeId, dto);
            return Ok(true);
        }

        [HttpPost("unregister-device-token")]
        public async Task<ActionResult<bool>> UnregisterDeviceToken([FromBody] string token)
        {
            await _notificationService.UnregisterDeviceTokenAsync(token);
            return Ok(true);
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
