using System;
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
    [Route("api/v1/asset-type")]
    public class AssetTypesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AssetTypesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.AssetTypeView)]
        public async Task<ActionResult<PagedResult<AssetTypeDto>>> GetPaged([FromBody] GetAssetTypesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.AssetTypeView)]
        public async Task<ActionResult<AssetTypeDto>> GetDetail([FromBody] GetAssetTypeByIdQuery query)
        {
            AssetTypeDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy loại tài sản.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.AssetTypeCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateAssetTypeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.AssetTypeUpdate)]
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
        [RequirePermission(PermissionCodes.AssetTypeManage)]
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

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.AssetTypeImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadAssetTypeExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Loai_Tai_San.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.AssetTypeExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportAssetTypesExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Loai_Tai_San_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.AssetTypeImportExcel)]
        public async Task<ActionResult<AssetTypeImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            AssetTypeImportResultDto result = await _mediator.Send(new ImportAssetTypesExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
