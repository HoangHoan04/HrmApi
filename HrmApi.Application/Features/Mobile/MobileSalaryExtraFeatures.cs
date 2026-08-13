using System.Net;
using System.Text;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.Features.Salaries.Queries;
using MediatR;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMyPayslipHtmlQuery : MobilePayslipHtmlRequest, IRequest<MobilePayslipHtmlDto> { }

    public class GetMyPayslipHtmlQueryHandler : IRequestHandler<GetMyPayslipHtmlQuery, MobilePayslipHtmlDto>
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;

        public GetMyPayslipHtmlQueryHandler(IMediator mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        public async Task<MobilePayslipHtmlDto> Handle(GetMyPayslipHtmlQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUser.EmployeeId.HasValue || _currentUser.EmployeeId == Guid.Empty)
                throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");

            var detail = await _mediator.Send(new GetMySalaryDetailQuery
            {
                Id = request.Id,
                Year = request.Year,
                Month = request.Month,
            }, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy phiếu lương.");

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Payslip ")
              .Append(WebUtility.HtmlEncode(detail.PeriodCode)).Append("</title>")
              .Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:24px}table{border-collapse:collapse;width:100%}")
              .Append("th,td{border:1px solid #ddd;padding:8px;text-align:left}th{background:#f5f5f5}")
              .Append(".right{text-align:right}.muted{color:#666}</style></head><body>");
            sb.Append("<h2>Phiếu lương ").Append(WebUtility.HtmlEncode(detail.PeriodCode)).Append("</h2>");
            sb.Append("<p><strong>Nhân viên:</strong> ").Append(WebUtility.HtmlEncode(detail.EmployeeName ?? ""))
              .Append(" (").Append(WebUtility.HtmlEncode(detail.EmployeeCode ?? "")).Append(")</p>");
            sb.Append("<p class='muted'>Trạng thái: ").Append(WebUtility.HtmlEncode(detail.Status))
              .Append(" · Công: ").Append(detail.ActualWorkingDays).Append("/").Append(detail.StandardWorkingDays).Append("</p>");
            sb.Append("<table><thead><tr><th>Loại</th><th>Mã</th><th>Khoản mục</th><th class='right'>Số tiền</th></tr></thead><tbody>");
            foreach (var line in detail.LineItems.OrderBy(x => x.DisplayOrder))
            {
                sb.Append("<tr><td>").Append(WebUtility.HtmlEncode(line.ItemType)).Append("</td><td>")
                  .Append(WebUtility.HtmlEncode(line.ItemCode)).Append("</td><td>")
                  .Append(WebUtility.HtmlEncode(line.ItemName)).Append("</td><td class='right'>")
                  .Append(line.Amount.ToString("N0")).Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Append("<p><strong>Gross:</strong> ").Append(detail.GrossSalary.ToString("N0"))
              .Append(" · <strong>Khấu trừ:</strong> ").Append(detail.TotalDeduction.ToString("N0"))
              .Append(" · <strong>Net:</strong> ").Append(detail.NetSalary.ToString("N0"))
              .Append(" ").Append(WebUtility.HtmlEncode(detail.Currency)).Append("</p>");
            sb.Append("<p class='muted'>In trang này để lưu PDF.</p></body></html>");

            return new MobilePayslipHtmlDto
            {
                SalaryId = detail.Id,
                Html = sb.ToString(),
            };
        }
    }
}
