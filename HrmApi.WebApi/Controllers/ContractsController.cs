using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Contract;
using HrmApi.Application.Features.Contracts.Commands;
using HrmApi.Application.Features.Contracts.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/contract")]
    public class ContractsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContractsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.HrContractView)]
        public async Task<ActionResult<PagedResult<ContractDto>>> GetPaged([FromBody] GetContractsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.HrContractView)]
        public async Task<ActionResult<ContractDto>> GetDetail([FromBody] GetContractByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy hợp đồng.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.HrContractCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateContractCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.HrContractUpdate)]
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
        [RequirePermission(PermissionCodes.HrContractSign)]
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
        [RequirePermission(PermissionCodes.HrContractTerminate)]
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
        [RequirePermission(PermissionCodes.HrContractRenew)]
        public async Task<ActionResult<Guid>> Renew([FromBody] RenewContractCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("history")]
        [RequirePermission(PermissionCodes.HrContractView)]
        public async Task<ActionResult<List<ContractDto>>> History([FromBody] GetContractHistoryQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("expiring-soon")]
        [RequirePermission(PermissionCodes.HrContractView)]
        public async Task<ActionResult<List<ContractDto>>> ExpiringSoon([FromBody] GetExpiringSoonContractsQuery query)
            => Ok(await _mediator.Send(query));
    }
}
