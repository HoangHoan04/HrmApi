using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.DayOffConfig;
using HrmApi.Application.Features.DayOffConfigs.Commands;
using HrmApi.Application.Features.DayOffConfigs.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/day-off-config")]
    public class DayOffConfigsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DayOffConfigsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigView)]
        public async Task<ActionResult<PagedResult<DayOffConfigDto>>> GetPaged([FromBody] GetDayOffConfigsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigView)]
        public async Task<ActionResult<DayOffConfigDto>> GetDetail([FromBody] GetDayOffConfigByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy cấu hình nghỉ phép.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateDayOffConfigCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateDayOffConfigCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy cấu hình nghỉ phép.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateDayOffConfigCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình nghỉ phép.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateDayOffConfigCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình nghỉ phép.");
            return Ok(result);
        }

        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigView)]
        public async Task<ActionResult<List<DayOffConfigSelectBoxDto>>> GetSelectBox([FromBody] GetDayOffConfigSelectBoxQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadDayOffConfigExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Cau_Hinh_Nghi_Phep.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportDayOffConfigsExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Cau_Hinh_Nghi_Phep_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.OperateDayOffConfigImportExcel)]
        public async Task<ActionResult<DayOffConfigImportResultDto>> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");
            }

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            DayOffConfigImportResultDto result = await _mediator.Send(new ImportDayOffConfigsExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
