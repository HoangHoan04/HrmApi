using HrmApi.Application.Common.Constants;
using HrmApi.Application.Features.EmployeeWorkPatterns;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/employee-work-pattern")]
    public class EmployeeWorkPatternsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EmployeeWorkPatternsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateWorkPatternView)]
        public async Task<ActionResult> Pagination([FromBody] GetEmployeeWorkPatternsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("upsert")]
        [RequirePermission(PermissionCodes.OperateWorkPatternManage)]
        public async Task<ActionResult<Guid>> Upsert([FromBody] UpsertEmployeeWorkPatternCommand command)
            => Ok(await _mediator.Send(command));

        [HttpPost("bulk-upsert")]
        [RequirePermission(PermissionCodes.OperateWorkPatternBulkAssign)]
        public async Task<ActionResult<BulkEmployeeWorkPatternResult>> BulkUpsert(
            [FromBody] BulkUpsertEmployeeWorkPatternCommand command)
            => Ok(await _mediator.Send(command));

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OperateWorkPatternDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateEmployeeWorkPatternCommand command)
            => Ok(await _mediator.Send(command));

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.OperateWorkPatternImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadEmployeeWorkPatternExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Mau_Phan_Ca.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.OperateWorkPatternExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportEmployeeWorkPatternsExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Mau_Phan_Ca_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.OperateWorkPatternImportExcel)]
        public async Task<ActionResult<EmployeeWorkPatternImportResultDto>> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");
            }

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            EmployeeWorkPatternImportResultDto result = await _mediator.Send(new ImportEmployeeWorkPatternsExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
