using System;
using System.Threading.Tasks;
using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Application.Features.MobileTimekeeping.Commands;
using HrmApi.Application.Features.MobileTimekeeping.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/mobile/timekeeping")]
    public class MobileTimekeepingController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileTimekeepingController(IMediator mediator) => _mediator = mediator;

        [HttpPost("today")]
        public async Task<ActionResult<MobileTodayDto>> Today([FromBody] GetMobileTodayQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMobileTodayQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("check-in")]
        public async Task<ActionResult<MobileTodayDto>> CheckIn([FromBody] MobileCheckInCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("check-out")]
        public async Task<ActionResult<MobileTodayDto>> CheckOut([FromBody] MobileCheckOutCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("month")]
        public async Task<ActionResult<MobileMonthDto>> Month([FromBody] GetMobileMonthQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
