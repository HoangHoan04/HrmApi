using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.TransferEmployee;
using HrmApi.Application.Features.TransferEmployees.Commands;
using HrmApi.Application.Features.TransferEmployees.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/transfer-employee")]
    public class TransferEmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TransferEmployeesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<TransferEmployeeDto>>> GetPaged([FromBody] GetTransferEmployeesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<TransferEmployeeDto>> GetDetail([FromBody] GetTransferEmployeeByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy đơn điều chuyển.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateTransferEmployeeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        public async Task<ActionResult<bool>> Approve([FromBody] ApproveTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        public async Task<ActionResult<bool>> Reject([FromBody] RejectTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("apply")]
        public async Task<ActionResult<bool>> Apply([FromBody] ApplyTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("history")]
        public async Task<ActionResult<List<TransferEmployeeDto>>> History([FromBody] GetTransferEmployeeHistoryQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("employee-org-snapshot")]
        public async Task<ActionResult<TransferEmployeePositionDto>> EmployeeOrgSnapshot([FromBody] GetEmployeeOrgSnapshotQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy nhân viên.");
            return Ok(result);
        }
    }
}
