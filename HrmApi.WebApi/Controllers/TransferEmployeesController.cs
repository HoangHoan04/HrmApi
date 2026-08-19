using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.TransferEmployee;
using HrmApi.Application.Features.TransferEmployees.Commands;
using HrmApi.Application.Features.TransferEmployees.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/transfer-employee")]
    public class TransferEmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TransferEmployeesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.HrTransferView)]
        public async Task<ActionResult<PagedResult<TransferEmployeeDto>>> GetPaged([FromBody] GetTransferEmployeesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.HrTransferView)]
        public async Task<ActionResult<TransferEmployeeDto>> GetDetail([FromBody] GetTransferEmployeeByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy đơn điều chuyển.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.HrTransferCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateTransferEmployeeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("bulk-create")]
        [RequirePermission(PermissionCodes.HrTransferCreate)]
        public async Task<ActionResult<List<Guid>>> BulkCreate([FromBody] BulkCreateTransferEmployeeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.HrTransferUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.HrTransferApprove)]
        public async Task<ActionResult<bool>> Approve([FromBody] ApproveTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        [RequirePermission(PermissionCodes.HrTransferReject)]
        public async Task<ActionResult<bool>> Reject([FromBody] RejectTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("apply")]
        [RequirePermission(PermissionCodes.HrTransferApply)]
        public async Task<ActionResult<bool>> Apply([FromBody] ApplyTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.HrTransferCancel)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelTransferEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đơn điều chuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("history")]
        [RequirePermission(PermissionCodes.HrTransferView)]
        public async Task<ActionResult<List<TransferEmployeeDto>>> History([FromBody] GetTransferEmployeeHistoryQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("employee-org-snapshot")]
        [RequirePermission(PermissionCodes.HrTransferView)]
        public async Task<ActionResult<TransferEmployeePositionDto>> EmployeeOrgSnapshot([FromBody] GetEmployeeOrgSnapshotQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy nhân viên.");
            return Ok(result);
        }

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.HrTransferImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadTransferEmployeeExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Dieu_Chuyen_Nhan_Su.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.HrTransferExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportTransferEmployeesExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Dieu_Chuyen_Nhan_Su_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.HrTransferImportExcel)]
        public async Task<ActionResult<TransferEmployeeImportResultDto>> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");
            }

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            TransferEmployeeImportResultDto result = await _mediator.Send(new ImportTransferEmployeesExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
