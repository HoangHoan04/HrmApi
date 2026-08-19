using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.WorkSchedule;
using HrmApi.Application.Features.WorkSchedules.Commands;
using HrmApi.Application.Features.WorkSchedules.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/work-schedule")]
    public class WorkSchedulesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public WorkSchedulesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleView)]
        public async Task<ActionResult<PagedResult<WorkScheduleDto>>> GetPaged([FromBody] GetWorkSchedulesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleView)]
        public async Task<ActionResult<WorkScheduleDto>> GetDetail([FromBody] GetWorkScheduleByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy lịch làm việc.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateWorkScheduleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateWorkScheduleCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch làm việc.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateWorkScheduleCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy lịch làm việc.");
            return Ok(result);
        }

        [HttpPost("bulk-create")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleCreate)]
        public async Task<ActionResult<BulkWorkScheduleResult>> BulkCreate([FromBody] BulkCreateWorkScheduleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("copy-week")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleCreate)]
        public async Task<ActionResult<BulkWorkScheduleResult>> CopyWeek([FromBody] CopyWorkScheduleWeekCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadWorkScheduleExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Lich_Lam_Viec.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportWorkSchedulesExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Lich_Lam_Viec_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.OperateWorkScheduleImportExcel)]
        public async Task<ActionResult<WorkScheduleImportResultDto>> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");
            }

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            WorkScheduleImportResultDto result = await _mediator.Send(new ImportWorkSchedulesExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
