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
    [Route("api/v1/asset-ticket")]
    public class AssetTicketsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AssetTicketsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.AssetTicketView)]
        public async Task<ActionResult<PagedResult<AssetTicketDto>>> GetPaged([FromBody] GetAssetTicketsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.AssetTicketView)]
        public async Task<ActionResult<AssetTicketDto>> GetDetail([FromBody] GetAssetTicketByIdQuery query)
        {
            AssetTicketDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy phiếu tài sản.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.AssetTicketCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateAssetTicketCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.AssetTicketUpdate)]
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
        [RequirePermission(PermissionCodes.AssetTicketComplete)]
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

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.AssetTicketCancel)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelAssetTicketCommand command)
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
        [RequirePermission(PermissionCodes.AssetTicketManage)]
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

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.AssetTicketImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadAssetTicketExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Phieu_Tai_San.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.AssetTicketExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportAssetTicketsExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Phieu_Tai_San_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.AssetTicketImportExcel)]
        public async Task<ActionResult<AssetTicketImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            AssetTicketImportResultDto result = await _mediator.Send(new ImportAssetTicketsExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
