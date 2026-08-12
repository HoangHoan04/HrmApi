using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.ReviewRenewal;
using HrmApi.Application.Features.ReviewRenewals.Commands;
using HrmApi.Application.Features.ReviewRenewals.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/review-renewal")]
    public class ReviewRenewalsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ReviewRenewalsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<ReviewRenewalDto>>> GetPaged([FromBody] GetReviewRenewalsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<ReviewRenewalDto>> GetDetail([FromBody] GetReviewRenewalByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy đánh giá gia hạn.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateReviewRenewalCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateReviewRenewalCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đánh giá gia hạn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        public async Task<ActionResult<bool>> Approve([FromBody] ApproveReviewRenewalCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đánh giá gia hạn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        public async Task<ActionResult<bool>> Reject([FromBody] RejectReviewRenewalCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đánh giá gia hạn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("apply")]
        public async Task<ActionResult<Guid?>> Apply([FromBody] ApplyReviewRenewalCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
