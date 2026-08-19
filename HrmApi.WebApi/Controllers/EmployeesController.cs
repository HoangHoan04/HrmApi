using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Employee;
using HrmApi.Application.Features.Employees.Commands;
using HrmApi.Application.Features.Employees.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API quản lý danh sách nhân viên
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/employee")]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmployeesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.HrEmployeeView)]
        public async Task<ActionResult<PagedResult<EmployeeDto>>> GetPagedList([FromBody] GetEmployeesPagedQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.HrEmployeeView)]
        public async Task<ActionResult<EmployeeDto>> GetDetail([FromBody] GetEmployeeByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin nhân viên.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.HrEmployeeCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateEmployeeCommand command)
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

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin nhân viên cần cập nhật.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.HrEmployeeActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateEmployeeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin nhân viên.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.HrEmployeeDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateEmployeeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin nhân viên.");
            return Ok(result);
        }

        [HttpPost("set-lifecycle-status")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<bool>> SetLifecycleStatus([FromBody] SetEmployeeLifecycleStatusCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin nhân viên.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("bulk-change-manager")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<int>> BulkChangeManager([FromBody] BulkChangeDirectManagerCommand command)
        {
            try
            {
                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("change-timeline")]
        [RequirePermission(PermissionCodes.HrEmployeeView)]
        public async Task<ActionResult<List<EmployeeChangeTimelineItemDto>>> ChangeTimeline(
            [FromBody] GetEmployeeChangeTimelineQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("files/expiring")]
        [RequirePermission(PermissionCodes.HrEmployeeView)]
        public async Task<ActionResult<List<EmployeeExpiringFileDto>>> ExpiringFiles(
            [FromBody] GetExpiringEmployeeFilesQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.HrEmployeeView)]
        public async Task<ActionResult<List<EmployeeSelectBoxDto>>> GetSelectBox([FromBody] GetEmployeeSelectBoxQuery? query)
        {
            var result = await _mediator.Send(query ?? new GetEmployeeSelectBoxQuery());
            return Ok(result);
        }

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.HrEmployeeImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            var content = await _mediator.Send(new DownloadEmployeeExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Nhan_Vien.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.HrEmployeeExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportEmployeesExcelQuery query)
        {
            var content = await _mediator.Send(query);
            var fileName = $"Danh_Sach_Nhan_Vien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.HrEmployeeImportExcel)]
        public async Task<ActionResult<EmployeeImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var result = await _mediator.Send(new ImportEmployeesExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }

        #region Dependent
        [HttpPost("dependent/create")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<Guid>> CreateDependent([FromBody] CreateEmployeeDependentCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("dependent/update")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<bool>> UpdateDependent([FromBody] UpdateEmployeeDependentCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy người phụ thuộc.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("dependent/delete")]
        [RequirePermission(PermissionCodes.HrEmployeeManage)]
        public async Task<ActionResult<bool>> DeleteDependent([FromBody] DeleteEmployeeDependentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy người phụ thuộc.");
            return Ok(result);
        }
        #endregion

        #region Education
        [HttpPost("education/create")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<Guid>> CreateEducation([FromBody] CreateEmployeeEducationCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("education/update")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<bool>> UpdateEducation([FromBody] UpdateEmployeeEducationCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy học vấn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("education/delete")]
        [RequirePermission(PermissionCodes.HrEmployeeManage)]
        public async Task<ActionResult<bool>> DeleteEducation([FromBody] DeleteEmployeeEducationCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy học vấn.");
            return Ok(result);
        }
        #endregion

        #region Certificate
        [HttpPost("certificate/create")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<Guid>> CreateCertificate([FromBody] CreateEmployeeCertificateCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("certificate/update")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<bool>> UpdateCertificate([FromBody] UpdateEmployeeCertificateCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy chứng chỉ.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("certificate/delete")]
        [RequirePermission(PermissionCodes.HrEmployeeManage)]
        public async Task<ActionResult<bool>> DeleteCertificate([FromBody] DeleteEmployeeCertificateCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy chứng chỉ.");
            return Ok(result);
        }
        #endregion

        #region File
        [HttpPost("file/create")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<Guid>> CreateFile([FromBody] CreateEmployeeFileCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("file/update")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<bool>> UpdateFile([FromBody] UpdateEmployeeFileCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy tài liệu.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("file/delete")]
        [RequirePermission(PermissionCodes.HrEmployeeManage)]
        public async Task<ActionResult<bool>> DeleteFile([FromBody] DeleteEmployeeFileCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy tài liệu.");
            return Ok(result);
        }
        #endregion

        #region Salary History
        [HttpPost("salary-history/create")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<Guid>> CreateSalaryHistory([FromBody] CreateEmployeeSalaryHistoryCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("salary-history/update")]
        [RequirePermission(PermissionCodes.HrEmployeeUpdate)]
        public async Task<ActionResult<bool>> UpdateSalaryHistory([FromBody] UpdateEmployeeSalaryHistoryCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch sử lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("salary-history/delete")]
        [RequirePermission(PermissionCodes.HrEmployeeManage)]
        public async Task<ActionResult<bool>> DeleteSalaryHistory([FromBody] DeleteEmployeeSalaryHistoryCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy lịch sử lương.");
            return Ok(result);
        }
        #endregion

        #region Assets
        [HttpPost("assets")]
        [RequirePermission(PermissionCodes.HrEmployeeView)]
        public async Task<ActionResult<HrmApi.Application.DTOs.Asset.EmployeeAssetSummaryDto>> GetEmployeeAssets(
            [FromBody] HrmApi.Application.Features.Asset.Queries.GetEmployeeAssetsQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin nhân viên.");
            return Ok(result);
        }

        [HttpPost("asset-clearance")]
        [RequirePermission(PermissionCodes.HrEmployeeView)]
        public async Task<ActionResult<HrmApi.Application.DTOs.Asset.EmployeeAssetClearanceDto>> CheckAssetClearance(
            [FromBody] HrmApi.Application.Features.Asset.Queries.CheckEmployeeAssetClearanceQuery query)
            => Ok(await _mediator.Send(query));
        #endregion
    }
}
