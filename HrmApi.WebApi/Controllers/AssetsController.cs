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
    [Route("api/v1/asset")]
    public class AssetsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AssetsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.AssetInventoryView)]
        public async Task<ActionResult<PagedResult<AssetDto>>> GetPaged([FromBody] GetAssetsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.AssetInventoryView)]
        public async Task<ActionResult<AssetDto>> GetDetail([FromBody] GetAssetByIdQuery query)
        {
            AssetDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy tài sản.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.AssetInventoryCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateAssetCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.AssetInventoryUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateAssetCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy tài sản.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.AssetInventoryManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteAssetCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy tài sản.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
