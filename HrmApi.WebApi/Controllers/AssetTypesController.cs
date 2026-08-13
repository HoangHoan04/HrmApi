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
    [Route("api/v1/asset-type")]
    public class AssetTypesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AssetTypesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.AssetInventoryView)]
        public async Task<ActionResult<PagedResult<AssetTypeDto>>> GetPaged([FromBody] GetAssetTypesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.AssetInventoryView)]
        public async Task<ActionResult<AssetTypeDto>> GetDetail([FromBody] GetAssetTypeByIdQuery query)
        {
            AssetTypeDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy loại tài sản.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.AssetInventoryCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateAssetTypeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.AssetInventoryUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateAssetTypeCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy loại tài sản.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.AssetInventoryManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteAssetTypeCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy loại tài sản.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
