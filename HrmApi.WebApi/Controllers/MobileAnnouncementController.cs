using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.Features.Mobile;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [RequirePermission(PermissionCodes.MobileAccess)]
    [Route("api/v1/mobile/announcements")]
    public class MobileAnnouncementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileAnnouncementController(IMediator mediator) => _mediator = mediator;

        [HttpPost("my-list")]
        public async Task<ActionResult<List<MobileAnnouncementDto>>> MyList([FromBody] GetMyAnnouncementsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyAnnouncementsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateMobileAnnouncementCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
