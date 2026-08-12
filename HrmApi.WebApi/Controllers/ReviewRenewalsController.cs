using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.ReviewRenewal;
using HrmApi.Application.Features.ReviewRenewals.Commands;
using HrmApi.Application.Features.ReviewRenewals.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/review-renewal")]
    public class ReviewRenewalsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ReviewRenewalsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.HrReviewRenewalView)]
        public async Task<ActionResult<PagedResult<ReviewRenewalDto>>> GetPaged([FromBody] GetReviewRenewalsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.HrReviewRenewalView)]
        public async Task<ActionResult<ReviewRenewalDto>> GetDetail([FromBody] GetReviewRenewalByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy đánh giá gia hạn.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.HrReviewRenewalCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateReviewRenewalCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.HrReviewRenewalUpdate)]
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
        [RequirePermission(PermissionCodes.HrReviewRenewalApprove)]
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
        [RequirePermission(PermissionCodes.HrReviewRenewalReject)]
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
        [RequirePermission(PermissionCodes.HrReviewRenewalApply)]
        public async Task<ActionResult<Guid?>> Apply([FromBody] ApplyReviewRenewalCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
