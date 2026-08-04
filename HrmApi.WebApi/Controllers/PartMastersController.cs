using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.PartMaster;
using HrmApi.Application.Features.PartMasters.Commands;
using HrmApi.Application.Features.PartMasters.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API quản lý danh mục mẫu tổ/nhóm (Part Master)
    /// </summary>
    [ApiController]
    [Route("api/v1/part-master")]
    public class PartMastersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PartMastersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách mẫu tổ/nhóm phân trang (Phương thức POST)
        /// </summary>
        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<PartMasterDto>>> GetPagedList([FromBody] GetPartMastersPagedQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Chi tiết mẫu tổ/nhóm theo ID (Phương thức POST)
        /// </summary>
        [HttpPost("detail")]
        public async Task<ActionResult<PartMasterDto>> GetDetail([FromBody] GetPartMasterByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin mẫu tổ/nhóm.");
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới mẫu tổ/nhóm (Phương thức POST)
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePartMasterCommand command)
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
        /// Cập nhật mẫu tổ/nhóm (Phương thức POST)
        /// </summary>
        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdatePartMasterCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin mẫu tổ/nhóm cần cập nhật.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kích hoạt mẫu tổ/nhóm (Phương thức POST, sets IsDeleted = false)
        /// </summary>
        [HttpPost("activate")]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivatePartMasterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin mẫu tổ/nhóm.");
            return Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa/Ngừng hoạt động mẫu tổ/nhóm (Phương thức POST, sets IsDeleted = true)
        /// </summary>
        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivatePartMasterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin mẫu tổ/nhóm.");
            return Ok(result);
        }

        /// <summary>
        /// Lấy dữ liệu rút gọn phục vụ chọn lựa SelectBox (Phương thức POST)
        /// </summary>
        [HttpPost("select-box")]
        public async Task<ActionResult<List<PartMasterSelectBoxDto>>> GetSelectBox([FromBody] GetPartMasterSelectBoxQuery? query)
        {
            var result = await _mediator.Send(query ?? new GetPartMasterSelectBoxQuery());
            return Ok(result);
        }

        /// <summary>
        /// Tải file mẫu Excel import mẫu tổ/nhóm
        /// </summary>
        [HttpPost("excel/template")]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            var content = await _mediator.Send(new DownloadPartMasterExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Mau_To_Nhom.xlsx");
        }

        /// <summary>
        /// Xuất danh sách mẫu tổ/nhóm ra Excel
        /// </summary>
        [HttpPost("excel/export")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportPartMastersExcelQuery query)
        {
            var content = await _mediator.Send(query);
            var fileName = $"Danh_Sach_Mau_To_Nhom_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Import danh sách mẫu tổ/nhóm từ Excel
        /// </summary>
        [HttpPost("excel/import")]
        public async Task<ActionResult<PartMasterImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var result = await _mediator.Send(new ImportPartMastersExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}