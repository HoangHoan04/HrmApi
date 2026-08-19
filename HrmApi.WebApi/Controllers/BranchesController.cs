using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Branch;
using HrmApi.Application.Features.Branches.Commands;
using HrmApi.Application.Features.Branches.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API quản lý danh sách chi nhánh
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/branch")]
    public class BranchesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BranchesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách chi nhánh phân trang (Phương thức POST)
        /// </summary>
        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OrgBranchView)]
        public async Task<ActionResult<PagedResult<BranchDto>>> GetPagedList([FromBody] GetBranchesPagedQuery query)
        {
            PagedResult<BranchDto> result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Chi tiết chi nhánh theo ID (Phương thức POST)
        /// </summary>
        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.OrgBranchView)]
        public async Task<ActionResult<BranchDto>> GetDetail([FromBody] GetBranchByIdQuery query)
        {
            BranchDto? result = await _mediator.Send(query);
            return result == null ? (ActionResult<BranchDto>)NotFound("Không tìm thấy thông tin chi nhánh.") : (ActionResult<BranchDto>)Ok(result);
        }

        /// <summary>
        /// Thêm mới chi nhánh (Phương thức POST)
        /// </summary>
        [HttpPost("create")]
        [RequirePermission(PermissionCodes.OrgBranchCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateBranchCommand command)
        {
            try
            {
                Guid result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật chi nhánh (Phương thức POST)
        /// </summary>
        [HttpPost("update")]
        [RequirePermission(PermissionCodes.OrgBranchUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateBranchCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                return !result ? (ActionResult<bool>)NotFound("Không tìm thấy thông tin chi nhánh cần cập nhật.") : (ActionResult<bool>)Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kích hoạt chi nhánh (Phương thức POST, sets IsDeleted = false)
        /// </summary>
        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.OrgBranchActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateBranchCommand command)
        {
            bool result = await _mediator.Send(command);
            return !result ? (ActionResult<bool>)NotFound("Không tìm thấy thông tin chi nhánh.") : (ActionResult<bool>)Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa/Ngừng hoạt động chi nhánh (Phương thức POST, sets IsDeleted = true)
        /// </summary>
        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OrgBranchDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateBranchCommand command)
        {
            bool result = await _mediator.Send(command);
            return !result ? (ActionResult<bool>)NotFound("Không tìm thấy thông tin chi nhánh.") : (ActionResult<bool>)Ok(result);
        }

        /// <summary>
        /// Lấy dữ liệu rút gọn phục vụ chọn lựa SelectBox (Phương thức POST)
        /// </summary>
        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.OrgBranchView)]
        public async Task<ActionResult<List<BranchSelectBoxDto>>> GetSelectBox([FromBody] GetBranchSelectBoxQuery query)
        {
            List<BranchSelectBoxDto> result = await _mediator.Send(query);
            return Ok(result);
        }


        /// <summary>
        /// Tải file mẫu Excel import chi nhánh
        /// </summary>
        [HttpPost("excel/template")]
        [RequirePermission(PermissionCodes.OrgBranchImportExcel)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            byte[] content = await _mediator.Send(new DownloadBranchExcelTemplateQuery());
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Chi_Nhanh.xlsx");
        }



        [HttpPost("excel/export")]
        [RequirePermission(PermissionCodes.OrgBranchExportExcel)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportBranchesExcelQuery query)
        {
            byte[] content = await _mediator.Send(query);
            string fileName = $"Danh_Sach_Chi_Nhanh_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Import danh sách chi nhánh từ Excel
        /// </summary>
        [HttpPost("excel/import")]
        [RequirePermission(PermissionCodes.OrgBranchImportExcel)]
        public async Task<ActionResult<BranchImportResultDto>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file Excel hợp lệ.");
            }

            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            BranchImportResultDto result = await _mediator.Send(new ImportBranchesExcelCommand
            {
                FileContent = memoryStream.ToArray()
            });

            return Ok(result);
        }

        /// <summary>
        /// Load chi nhánh theo id công ty (dùng cho cascade dropdown).
        /// </summary>
        [HttpPost("load-by-company")]
        [RequirePermission(PermissionCodes.OrgBranchView)]
        public async Task<ActionResult<List<BranchSelectBoxDto>>> GetByCompany(
            [FromBody] GetBranchesByCompanyQuery query)
        {
            try
            {
                return Ok(await _mediator.Send(query));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
