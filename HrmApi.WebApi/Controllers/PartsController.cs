using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Part;
using HrmApi.Application.Features.Parts.Commands;
using HrmApi.Application.Features.Parts.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API quản lý danh sách bộ phận
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/part")]
    public class PartsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách bộ phận phân trang (Phương thức POST)
        /// </summary>
        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OrgPartView)]
        public async Task<ActionResult<PagedResult<PartDto>>> GetPagedList([FromBody] GetPartsPagedQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Chi tiết bộ phận theo ID (Phương thức POST)
        /// </summary>
        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OrgPartView)]
        public async Task<ActionResult<PartDto>> GetDetail([FromBody] GetPartByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin bộ phận.");
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới bộ phận (Phương thức POST)
        /// </summary>
        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OrgPartCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePartCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật bộ phận (Phương thức POST)
        /// </summary>
        [HttpPost("update")]
        [RequirePermission(PermissionCodes.OrgPartUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdatePartCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin bộ phận cần cập nhật.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kích hoạt bộ phận (Phương thức POST, sets IsDeleted = false)
        /// </summary>
        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.OrgPartActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivatePartCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin bộ phận.");
            return Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa/Ngừng hoạt động bộ phận (Phương thức POST, sets IsDeleted = true)
        /// </summary>
        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OrgPartDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivatePartCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin bộ phận.");
            return Ok(result);
        }

        /// <summary>
        /// Lấy dữ liệu rút gọn phục vụ chọn lựa SelectBox (Phương thức POST)
        /// </summary>
        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.OrgPartView)]
        public async Task<ActionResult<List<PartSelectBoxDto>>> GetSelectBox([FromBody] GetPartSelectBoxQuery? query)
        {
            var result = await _mediator.Send(query ?? new GetPartSelectBoxQuery());
            return Ok(result);
        }

        /// <summary>
        /// Tải file mẫu Excel import bộ phận
        /// </summary>
        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.OrgPartImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            var content = await _mediator.Send(new DownloadPartExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_To_Nhom.xlsx");
        }

        /// <summary>
        /// Xuất danh sách bộ phận ra Excel
        /// </summary>
        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.OrgPartExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportPartsExcelQuery query)
        {
            var content = await _mediator.Send(query);
            var fileName = $"Danh_Sach_To_Nhom_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Import danh sách bộ phận từ Excel
        /// </summary>
        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.OrgPartImportExcel)]
        public async Task<ActionResult<PartImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var result = await _mediator.Send(new ImportPartsExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }

        /// <summary>
        /// Load bộ phận/tổ theo id phòng ban (dùng cho cascade dropdown).
        /// </summary>
        [HttpPost("load-by-department")]
        [RequirePermission(PermissionCodes.OrgPartView)]
        public async Task<ActionResult<List<PartSelectBoxDto>>> GetByDepartment(
            [FromBody] GetPartsByDepartmentQuery query)
        {
            try
            {
                return Ok(await _mediator.Send(query));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
