using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Features.Integrations;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/timekeeping-punch")]
    public class TimekeepingPunchController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TimekeepingPunchController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Import punch CSV / JSON lines. Columns: EmployeeCode, PunchTime, Type (IN/OUT).
        /// Multipart field "file", raw text body, or JSON { content | rows }.
        /// </summary>
        [HttpPost("import-csv")]
        [RequirePermission(PermissionCodes.OperateTimekeepingManage)]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult<ImportPunchCsvResultDto>> ImportCsv()
        {
            try
            {
                var command = new ImportPunchCsvCommand();

                if (Request.HasFormContentType)
                {
                    IFormFile? file = Request.Form.Files.GetFile("file") ?? Request.Form.Files.FirstOrDefault();
                    if (file == null || file.Length == 0)
                        return BadRequest("Thiếu file CSV (field name: file).");
                    using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
                    command.Content = await reader.ReadToEndAsync();
                }
                else
                {
                    using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                    string body = await reader.ReadToEndAsync();
                    if (string.IsNullOrWhiteSpace(body))
                        return BadRequest("Cần upload file CSV hoặc gửi content/rows JSON.");

                    string trimmed = body.TrimStart();
                    if (trimmed.StartsWith('{'))
                    {
                        ImportPunchCsvCommand? parsed = JsonSerializer.Deserialize<ImportPunchCsvCommand>(
                            body,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (parsed != null) command = parsed;
                        else command.Content = body;
                    }
                    else
                    {
                        command.Content = body;
                    }
                }

                if (string.IsNullOrWhiteSpace(command.Content) && (command.Rows == null || command.Rows.Count == 0))
                    return BadRequest("Cần upload file CSV hoặc gửi content/rows JSON.");

                return Ok(await _mediator.Send(command));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
