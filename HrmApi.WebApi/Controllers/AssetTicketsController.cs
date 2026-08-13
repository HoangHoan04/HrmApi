using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Asset;
using HrmApi.Application.Features.Asset;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/asset-ticket")]
    public class AssetTicketsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AssetTicketsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.AssetView)]
        public async Task<ActionResult<PagedResult<AssetTicketDto>>> GetPaged([FromBody] GetAssetTicketsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.AssetView)]
        public async Task<ActionResult<AssetTicketDto>> GetDetail([FromBody] GetAssetTicketByIdQuery query)
        {
            AssetTicketDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy phiếu tài sản.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.AssetManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateAssetTicketCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.AssetManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateAssetTicketCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu tài sản.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("complete")]
        [RequirePermission(PermissionCodes.AssetManage)]
        public async Task<ActionResult<bool>> Complete([FromBody] CompleteAssetTicketCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu tài sản.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.AssetManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteAssetTicketCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu tài sản.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
