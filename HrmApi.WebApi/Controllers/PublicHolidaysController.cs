using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.PublicHoliday;
using HrmApi.Application.Features.PublicHolidays.Commands;
using HrmApi.Application.Features.PublicHolidays.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/public-holiday")]
    public class PublicHolidaysController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicHolidaysController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayView)]
        public async Task<ActionResult<PagedResult<PublicHolidayDto>>> GetPaged([FromBody] GetPublicHolidaysPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayView)]
        public async Task<ActionResult<PublicHolidayDto>> GetDetail([FromBody] GetPublicHolidayByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy ngày nghỉ lễ.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePublicHolidayCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdatePublicHolidayCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy ngày nghỉ lễ.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivatePublicHolidayCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy ngày nghỉ lễ.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivatePublicHolidayCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy ngày nghỉ lễ.");
            return Ok(result);
        }

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadPublicHolidayExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Ngay_Nghi_Le.xlsx");
        }

        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportPublicHolidaysExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Ngay_Nghi_Le_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.OperatePublicHolidayImportExcel)]
        public async Task<ActionResult<PublicHolidayImportResultDto>> ImportExcel(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");
            }

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            PublicHolidayImportResultDto result = await _mediator.Send(new ImportPublicHolidaysExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
