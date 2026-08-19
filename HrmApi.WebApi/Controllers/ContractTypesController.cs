using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.ContractType;
using HrmApi.Application.Features.ContractType.Commands;
using HrmApi.Application.Features.ContractType.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/contract-type")]
    public class ContractTypesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContractTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.HrContractTypeView)]
        public async Task<ActionResult<PagedResult<ContractTypeDto>>> GetPaged([FromBody] GetContractTypesPagedQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.HrContractTypeView)]
        public async Task<ActionResult<ContractTypeDto>> GetDetail([FromBody] GetContractTypeByIdQuery query)
        {
            ContractTypeDto? result = await _mediator.Send(query);
            return result == null ? (ActionResult<ContractTypeDto>)NotFound("Không tìm thấy loại hợp đồng.") : (ActionResult<ContractTypeDto>)Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.HrContractTypeCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateContractTypeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.HrContractTypeUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateContractTypeCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                return !result ? (ActionResult<bool>)NotFound("Không tìm thấy loại hợp đồng.") : (ActionResult<bool>)Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.HrContractTypeActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateContractTypeCommand command)
        {
            bool result = await _mediator.Send(command);
            return !result ? (ActionResult<bool>)NotFound("Không tìm thấy loại hợp đồng.") : (ActionResult<bool>)Ok(result);
        }

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.HrContractTypeDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateContractTypeCommand command)
        {
            bool result = await _mediator.Send(command);
            return !result ? (ActionResult<bool>)NotFound("Không tìm thấy loại hợp đồng.") : (ActionResult<bool>)Ok(result);
        }

        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.HrContractTypeView)]
        public async Task<ActionResult<List<ContractTypeSelectBoxDto>>> GetSelectBox([FromBody] GetContractTypeSelectBoxQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.HrContractTypeImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadContractTypesExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Loai_Hop_Dong.xlsx");
        }

        /// <summary>
        /// Xuất danh sách chi nhánh ra Excel
        /// </summary>
        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.HrContractTypeExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportContractTypesExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Loai_Hop_Dong_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.HrContractTypeImportExcel)]
        public async Task<ActionResult<ContractTypeImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");
            }

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            ContractTypeImportResultDto result = await _mediator.Send(new ImportContractTypesExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }

    }
}

