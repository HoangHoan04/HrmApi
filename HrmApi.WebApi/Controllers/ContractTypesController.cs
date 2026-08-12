using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.ContractType;
using HrmApi.Application.Features.ContractType.Commands;
using HrmApi.Application.Features.ContractType.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/contract-type")]
    public class ContractTypesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContractTypesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<ContractTypeDto>>> GetPaged([FromBody] GetContractTypesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        public async Task<ActionResult<ContractTypeDto>> GetDetail([FromBody] GetContractTypeByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy loại hợp đồng.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateContractTypeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateContractTypeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy loại hợp đồng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateContractTypeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy loại hợp đồng.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateContractTypeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy loại hợp đồng.");
            return Ok(result);
        }

        [HttpPost("select-box")]
        public async Task<ActionResult<List<ContractTypeSelectBoxDto>>> GetSelectBox([FromBody] GetContractTypeSelectBoxQuery query)
            => Ok(await _mediator.Send(query));
    }
}
