using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Company;
using HrmApi.Application.Features.Companies.Commands;
using HrmApi.Application.Features.Companies.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API quản lý danh sách công ty
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/company")]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompaniesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách công ty phân trang (Phương thức POST)
        /// </summary>
        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OrgCompanyView)]
        public async Task<ActionResult<PagedResult<CompanyDto>>> GetPagedList([FromBody] GetCompaniesPagedQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Chi tiết công ty theo ID (Phương thức POST)
        /// </summary>
        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OrgCompanyView)]
        public async Task<ActionResult<CompanyDto>> GetDetail([FromBody] GetCompanyByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin công ty.");
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới công ty (Phương thức POST)
        /// </summary>
        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OrgCompanyCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCompanyCommand command)
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
        /// Cập nhật công ty (Phương thức POST)
        /// </summary>
        [HttpPost("update")]
        [RequirePermission(PermissionCodes.OrgCompanyUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateCompanyCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin công ty cần cập nhật.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kích hoạt công ty (Phương thức POST, sets IsDeleted = false)
        /// </summary>
        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.OrgCompanyActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateCompanyCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin công ty.");
            return Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa/Ngừng hoạt động công ty (Phương thức POST, sets IsDeleted = true)
        /// </summary>
        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OrgCompanyDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateCompanyCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin công ty.");
            return Ok(result);
        }

        /// <summary>
        /// Lấy dữ liệu rút gọn phục vụ chọn lựa SelectBox (Phương thức POST)
        /// </summary>
        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.OrgCompanyView)]
        public async Task<ActionResult<List<CompanySelectBoxDto>>> GetSelectBox([FromBody] GetCompanySelectBoxQuery? query)
        {
            var result = await _mediator.Send(query ?? new GetCompanySelectBoxQuery());
            return Ok(result);
        }

        /// <summary>
        /// Tải file mẫu Excel import công ty
        /// </summary>
        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.OrgCompanyImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            var content = await _mediator.Send(new DownloadCompanyExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Cong_Ty.xlsx");
        }

        /// <summary>
        /// Xuất danh sách công ty ra Excel
        /// </summary>
        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.OrgCompanyExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportCompaniesExcelQuery query)
        {
            var content = await _mediator.Send(query);
            var fileName = $"Danh_Sach_Cong_Ty_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Import danh sách công ty từ Excel
        /// </summary>
        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.OrgCompanyImportExcel)]
        public async Task<ActionResult<CompanyImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var result = await _mediator.Send(new ImportCompaniesExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
