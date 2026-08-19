using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.TimeKeepingStandard;
using HrmApi.Application.Features.TimeKeepingStandards.Commands;
using HrmApi.Application.Features.TimeKeepingStandards.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/timekeeping-standard")]
    public class TimeKeepingStandardsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TimeKeepingStandardsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardView)]
        public async Task<ActionResult<PagedResult<TimeKeepingStandardDto>>> GetPaged([FromBody] GetTimeKeepingStandardsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardView)]
        public async Task<ActionResult<TimeKeepingStandardDto>> GetDetail([FromBody] GetTimeKeepingStandardByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy chuẩn chấm công.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateTimeKeepingStandardCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateTimeKeepingStandardCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy chuẩn chấm công.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateTimeKeepingStandardCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy chuẩn chấm công.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateTimeKeepingStandardCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy chuẩn chấm công.");
            return Ok(result);
        }

        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardView)]
        public async Task<ActionResult<List<TimeKeepingStandardSelectBoxDto>>> GetSelectBox([FromBody] GetTimeKeepingStandardSelectBoxQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadTimeKeepingStandardExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Chuan_Cham_Cong.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportTimeKeepingStandardsExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Chuan_Cham_Cong_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.OperateTimekeepingStandardImportExcel)]
        public async Task<ActionResult<TimeKeepingStandardImportResultDto>> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");
            }

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            TimeKeepingStandardImportResultDto result = await _mediator.Send(new ImportTimeKeepingStandardsExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
