using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Salary;
using HrmApi.Application.Features.Integrations;
using HrmApi.Application.Features.PayrollAdjustments;
using HrmApi.Application.Features.Salaries.Commands;
using HrmApi.Application.Features.Salaries.Queries;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Enums;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HrmApi.Application.Common.Interfaces;
using System.Text;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/salary")]
    public class SalariesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IApplicationDbContext _context;
        public SalariesController(IMediator mediator, IApplicationDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PayrollSalaryView)]
        public async Task<ActionResult<PagedResult<SalaryDto>>> GetPaged([FromBody] GetSalariesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.PayrollSalaryView)]
        public async Task<ActionResult<SalaryDto>> GetDetail([FromBody] GetSalaryByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy phiếu lương.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PayrollSalaryCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSalaryCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.PayrollSalaryUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateSalaryCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.PayrollSalaryApprove)]
        public async Task<ActionResult<bool>> Approve([FromBody] ApproveSalaryCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("mark-paid")]
        [RequirePermission(PermissionCodes.PayrollSalaryMarkPaid)]
        public async Task<ActionResult<bool>> MarkPaid([FromBody] MarkSalaryPaidCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.PayrollSalaryCancel)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelSalaryCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("preview-run")]
        [RequirePermission(PermissionCodes.PayrollSalaryCreate)]
        public async Task<ActionResult<PayrollPreviewResultDto>> PreviewRun([FromBody] PreviewPayrollRunQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("run")]
        [RequirePermission(PermissionCodes.PayrollSalaryCreate)]
        public async Task<ActionResult<PayrollRunResultDto>> Run([FromBody] RunPayrollCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("finalize-period")]
        [RequirePermission(PermissionCodes.PayrollSalaryApprove)]
        public async Task<ActionResult<int>> FinalizePeriod([FromBody] FinalizePayrollPeriodCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>Phiếu lương HTML in được (P1 stub PDF).</summary>
        [HttpGet("payslip-html")]
        [RequirePermission(PermissionCodes.PayrollSalaryView)]
        public async Task<IActionResult> PayslipHtml([FromQuery] Guid id, CancellationToken cancellationToken)
        {
            var entity = await _context.SalaryEntities.AsNoTracking()
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
            if (entity == null) return NotFound("Không tìm thấy phiếu lương.");

            var emp = await _context.EmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == entity.EmployeeId, cancellationToken);
            string empName = emp?.FullName ?? emp?.Code ?? entity.EmployeeId.ToString();
            var lines = (entity.LineItems ?? []).Where(x => !x.IsDeleted).OrderBy(x => x.DisplayOrder).ToList();

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Payslip ")
              .Append(entity.PeriodCode).Append("</title>")
              .Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:24px}table{border-collapse:collapse;width:100%}")
              .Append("th,td{border:1px solid #ddd;padding:8px;text-align:left}th{background:#f5f5f5}")
              .Append(".right{text-align:right}.muted{color:#666}</style></head><body>");
            sb.Append("<h2>Phiếu lương ").Append(entity.PeriodCode).Append("</h2>");
            sb.Append("<p><strong>Nhân viên:</strong> ").Append(System.Net.WebUtility.HtmlEncode(empName))
              .Append(" (").Append(System.Net.WebUtility.HtmlEncode(emp?.Code ?? "")).Append(")</p>");
            sb.Append("<p class='muted'>Trạng thái: ").Append(entity.Status)
              .Append(" · Công: ").Append(entity.ActualWorkingDays).Append("/").Append(entity.StandardWorkingDays).Append("</p>");
            sb.Append("<table><thead><tr><th>Loại</th><th>Mã</th><th>Khoản mục</th><th class='right'>Số tiền</th></tr></thead><tbody>");
            foreach (var line in lines)
            {
                sb.Append("<tr><td>").Append(line.ItemType).Append("</td><td>").Append(line.ItemCode)
                  .Append("</td><td>").Append(System.Net.WebUtility.HtmlEncode(line.ItemName))
                  .Append("</td><td class='right'>").Append(line.Amount.ToString("N0")).Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Append("<p><strong>Gross:</strong> ").Append(entity.GrossSalary.ToString("N0"))
              .Append(" · <strong>Khấu trừ:</strong> ").Append(entity.TotalDeduction.ToString("N0"))
              .Append(" · <strong>Net:</strong> ").Append(entity.NetSalary.ToString("N0")).Append(" ").Append(entity.Currency).Append("</p>");
            sb.Append("<p class='muted'>In trang này để lưu PDF (Ctrl+P).</p></body></html>");

            return Content(sb.ToString(), "text/html; charset=utf-8");
        }

        [HttpPost("export-bank-file")]
        [RequirePermission(PermissionCodes.PayrollSalaryView)]
        public async Task<IActionResult> ExportBankFile([FromBody] ExportBankPayrollFileCommand command)
        {
            try
            {
                SalaryExportFileResult file = await _mediator.Send(command);
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("export-bhxh")]
        [RequirePermission(PermissionCodes.PayrollSalaryView)]
        public async Task<IActionResult> ExportBhxh([FromBody] ExportBhxhFileCommand command)
        {
            try
            {
                SalaryExportFileResult file = await _mediator.Send(command);
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("export-accounting")]
        [RequirePermission(PermissionCodes.PayrollSalaryView)]
        public async Task<IActionResult> ExportAccounting([FromBody] ExportAccountingFileCommand command)
        {
            try
            {
                SalaryExportFileResult file = await _mediator.Send(command);
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }

    [ApiController]
    [Authorize]
    [Route("api/v1/allowance")]
    public class AllowancesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AllowancesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PayrollAllowanceView)]
        public async Task<ActionResult<PagedResult<AllowanceDto>>> Pagination([FromBody] GetAllowancesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("upsert")]
        [RequirePermission(PermissionCodes.PayrollAllowanceManage)]
        public async Task<ActionResult<Guid>> Upsert([FromBody] UpsertAllowanceCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("set-active")]
        [RequirePermission(PermissionCodes.PayrollAllowanceManage)]
        public async Task<ActionResult<bool>> SetActive([FromBody] SetAllowanceActiveCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }

    [ApiController]
    [Authorize]
    [Route("api/v1/advance")]
    public class AdvancesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdvancesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PayrollAdvanceView)]
        public async Task<ActionResult<PagedResult<AdvanceDto>>> Pagination([FromBody] GetAdvancesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PayrollAdvanceCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateAdvanceCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.PayrollAdvanceApprove)]
        public async Task<ActionResult<bool>> Approve([FromBody] ReviewAdvanceCommand command)
        {
            try { command.Approve = true; return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        [RequirePermission(PermissionCodes.PayrollAdvanceApprove)]
        public async Task<ActionResult<bool>> Reject([FromBody] ReviewAdvanceCommand command)
        {
            try { command.Approve = false; return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.PayrollAdvanceManage)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelAdvanceCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }

    [ApiController]
    [Authorize]
    [Route("api/v1/payroll-slip")]
    public class PayrollSlipsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PayrollSlipsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PayrollAdjustmentView)]
        public async Task<ActionResult<PagedResult<PayrollSlipDto>>> Pagination([FromBody] GetPayrollSlipsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PayrollAdjustmentManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePayrollSlipCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.PayrollAdjustmentManage)]
        public async Task<ActionResult<bool>> Approve([FromBody] ReviewPayrollSlipCommand command)
        {
            try { command.Approve = true; return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("reject")]
        [RequirePermission(PermissionCodes.PayrollAdjustmentManage)]
        public async Task<ActionResult<bool>> Reject([FromBody] ReviewPayrollSlipCommand command)
        {
            try { command.Approve = false; return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
