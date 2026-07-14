using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs;
using HrmApi.Application.Features.Companies.Commands;
using HrmApi.Application.Features.Companies.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API quản lý danh sách công ty
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompaniesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách công ty phân trang (Phương thức POST)
        /// </summary>
        [HttpPost("list")]
        public async Task<ActionResult<PagedResult<CompanyDto>>> GetPagedList([FromBody] GetCompaniesPagedQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Chi tiết công ty theo ID (Phương thức POST)
        /// </summary>
        [HttpPost("detail")]
        public async Task<ActionResult<CompanyDto>> GetDetail([FromBody] GetCompanyByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin công ty.");
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới công ty (Phương thức POST)
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCompanyCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật công ty (Phương thức POST)
        /// </summary>
        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateCompanyCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin công ty cần cập nhật.");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kích hoạt công ty (Phương thức POST, sets IsDeleted = false)
        /// </summary>
        [HttpPost("activate")]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateCompanyCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin công ty.");
            return Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa/Ngừng hoạt động công ty (Phương thức POST, sets IsDeleted = true)
        /// </summary>
        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateCompanyCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin công ty.");
            return Ok(result);
        }

        /// <summary>
        /// Lấy dữ liệu rút gọn phục vụ chọn lựa SelectBox (Phương thức POST)
        /// </summary>
        [HttpPost("select-box")]
        public async Task<ActionResult<List<CompanySelectBoxDto>>> GetSelectBox()
        {
            var result = await _mediator.Send(new GetCompanySelectBoxQuery());
            return Ok(result);
        }
    }
}
