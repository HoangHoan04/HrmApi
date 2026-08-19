using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Asset;
using HrmApi.Application.Features.Asset;
using HrmApi.Application.Features.Asset.Commands;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.AssetInventoryView)]
        public async Task<ActionResult<List<AssetSelectBoxDto>>> GetSelectBox([FromBody] GetAssetSelectBoxQuery query)
            => Ok(await _mediator.Send(query));

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

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.AssetInventoryImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadAssetExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Tai_San.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.AssetInventoryExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportAssetsExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Tai_San_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.AssetInventoryImportExcel)]
        public async Task<ActionResult<AssetImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            AssetImportResultDto result = await _mediator.Send(new ImportAssetsExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
