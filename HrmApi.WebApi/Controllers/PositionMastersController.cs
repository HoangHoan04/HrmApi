using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.PositionMaster;
using HrmApi.Application.Features.PositionMasters.Commands;
using HrmApi.Application.Features.PositionMasters.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/position-master")]
    public class PositionMastersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PositionMastersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("pagination")]
        public async Task<ActionResult<PagedResult<PositionMasterDto>>> GetPagedList([FromBody] GetPositionMastersPagedQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("detail")]
        public async Task<ActionResult<PositionMasterDto>> GetDetail([FromBody] GetPositionMasterByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin mẫu chức danh.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePositionMasterCommand command)
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

        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdatePositionMasterCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin mẫu chức danh cần cập nhật.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("activate")]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivatePositionMasterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin mẫu chức danh.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivatePositionMasterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin mẫu chức danh.");
            return Ok(result);
        }

        [HttpPost("select-box")]
        public async Task<ActionResult<List<PositionMasterSelectBoxDto>>> GetSelectBox([FromBody] GetPositionMasterSelectBoxQuery? query)
        {
            return Ok(await _mediator.Send(query ?? new GetPositionMasterSelectBoxQuery()));
        }

        [HttpPost("excel/template")]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            var content = await _mediator.Send(new DownloadPositionMasterExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Mau_Chuc_Danh.xlsx");
        }

        [HttpPost("excel/export")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportPositionMastersExcelQuery query)
        {
            var content = await _mediator.Send(query);
            var fileName = $"Danh_Sach_Mau_Chuc_Danh_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("excel/import")]
        public async Task<ActionResult<PositionMasterImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var result = await _mediator.Send(new ImportPositionMastersExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }
    }
}
