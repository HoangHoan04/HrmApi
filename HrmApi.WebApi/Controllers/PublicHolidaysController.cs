using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.PublicHoliday;
using HrmApi.Application.Features.PublicHolidays.Commands;
using HrmApi.Application.Features.PublicHolidays.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/public-holiday")]
    public class PublicHolidaysController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicHolidaysController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<PublicHolidayDto>>> GetPaged([FromBody] GetPublicHolidaysPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<PublicHolidayDto>> GetDetail([FromBody] GetPublicHolidayByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy ngày nghỉ lễ.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePublicHolidayCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdatePublicHolidayCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy ngày nghỉ lễ.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivatePublicHolidayCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy ngày nghỉ lễ.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivatePublicHolidayCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy ngày nghỉ lễ.");
            return Ok(result);
        }
    }
}
