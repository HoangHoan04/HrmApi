using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Department;
using HrmApi.Application.Features.Departments.Commands;
using HrmApi.Application.Features.Departments.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API quản lý danh sách phòng ban
    /// </summary>
    [ApiController]
    [Route("api/v1/department")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DepartmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách phòng ban phân trang (Phương thức POST)
        /// </summary>
        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<DepartmentDto>>> GetPagedList([FromBody] GetDepartmentsPagedQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Chi tiết phòng ban theo ID (Phương thức POST)
        /// </summary>
        [HttpPost("detail")]
        public async Task<ActionResult<DepartmentDto>> GetDetail([FromBody] GetDepartmentByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin phòng ban.");
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới phòng ban (Phương thức POST)
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateDepartmentCommand command)
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
        /// Cập nhật phòng ban (Phương thức POST)
        /// </summary>
        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateDepartmentCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin phòng ban cần cập nhật.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kích hoạt phòng ban (Phương thức POST, sets IsDeleted = false)
        /// </summary>
        [HttpPost("activate")]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateDepartmentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin phòng ban.");
            return Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa/Ngừng hoạt động phòng ban (Phương thức POST, sets IsDeleted = true)
        /// </summary>
        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateDepartmentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin phòng ban.");
            return Ok(result);
        }

        /// <summary>
        /// Lấy dữ liệu rút gọn phục vụ chọn lựa SelectBox (Phương thức POST)
        /// </summary>
        [HttpPost("select-box")]
        public async Task<ActionResult<List<DepartmentSelectBoxDto>>> GetSelectBox([FromBody] GetDepartmentSelectBoxQuery? query)
        {
            var result = await _mediator.Send(query ?? new GetDepartmentSelectBoxQuery());
            return Ok(result);
        }

        /// <summary>
        /// Tải file mẫu Excel import phòng ban
        /// </summary>
        [HttpPost("excel/template")]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            var content = await _mediator.Send(new DownloadDepartmentExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Phong_Ban.xlsx");
        }

        /// <summary>
        /// Xuất danh sách phòng ban ra Excel
        /// </summary>
        [HttpPost("excel/export")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportDepartmentsExcelQuery query)
        {
            var content = await _mediator.Send(query);
            var fileName = $"Danh_Sach_Phong_Ban_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Import danh sách phòng ban từ Excel
        /// </summary>
        [HttpPost("excel/import")]
        public async Task<ActionResult<DepartmentImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var result = await _mediator.Send(new ImportDepartmentsExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }

        /// <summary>
        /// Load phòng ban theo id chi nhánh (dùng cho cascade dropdown).
        /// </summary>
        [HttpPost("load-by-branch")]
        public async Task<ActionResult<List<DepartmentSelectBoxDto>>> GetByBranch(
            [FromBody] GetDepartmentsByBranchQuery query)
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