using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Contract;
using HrmApi.Application.Features.Contracts.Commands;
using HrmApi.Application.Features.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/contract")]
    public class ContractsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContractsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<ContractDto>>> GetPaged([FromBody] GetContractsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<ContractDto>> GetDetail([FromBody] GetContractByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy hợp đồng.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateContractCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateContractCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy hợp đồng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("sign")]
        public async Task<ActionResult<bool>> Sign([FromBody] SignContractCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy hợp đồng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("terminate")]
        public async Task<ActionResult<bool>> Terminate([FromBody] TerminateContractCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy hợp đồng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("renew")]
        public async Task<ActionResult<Guid>> Renew([FromBody] RenewContractCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("history")]
        public async Task<ActionResult<List<ContractDto>>> History([FromBody] GetContractHistoryQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("expiring-soon")]
        public async Task<ActionResult<List<ContractDto>>> ExpiringSoon([FromBody] GetExpiringSoonContractsQuery query)
            => Ok(await _mediator.Send(query));
    }
}
