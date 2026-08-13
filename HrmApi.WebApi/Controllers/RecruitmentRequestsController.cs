using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.Features.Recruitment;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/recruitment-request")]
    public class RecruitmentRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RecruitmentRequestsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.RecruitmentRequestView)]
        public async Task<ActionResult<PagedResult<RecruitmentRequestDto>>> GetPaged([FromBody] GetRecruitmentRequestsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.RecruitmentRequestView)]
        public async Task<ActionResult<RecruitmentRequestDto>> GetDetail([FromBody] GetRecruitmentRequestByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy yêu cầu tuyển dụng.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.RecruitmentRequestCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateRecruitmentRequestCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.RecruitmentRequestUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateRecruitmentRequestCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy yêu cầu tuyển dụng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("submit")]
        [RequirePermission(PermissionCodes.RecruitmentRequestUpdate)]
        public async Task<ActionResult<bool>> Submit([FromBody] SubmitRecruitmentRequestCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy yêu cầu tuyển dụng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.RecruitmentRequestApprove)]
        public async Task<ActionResult<bool>> Approve([FromBody] ApproveRecruitmentRequestCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy yêu cầu tuyển dụng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        [RequirePermission(PermissionCodes.RecruitmentRequestApprove)]
        public async Task<ActionResult<bool>> Reject([FromBody] RejectRecruitmentRequestCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy yêu cầu tuyển dụng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("close")]
        [RequirePermission(PermissionCodes.RecruitmentRequestUpdate)]
        public async Task<ActionResult<bool>> Close([FromBody] CloseRecruitmentRequestCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy yêu cầu tuyển dụng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
