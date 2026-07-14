using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs;
using HrmApi.Application.Features.Branches.Commands;
using HrmApi.Application.Features.Branches.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API quản lý danh sách chi nhánh
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
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
        [HttpPost("list")]
        public async Task<ActionResult<PagedResult<BranchDto>>> GetPagedList([FromBody] GetBranchesPagedQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Chi tiết chi nhánh theo ID (Phương thức POST)
        /// </summary>
        [HttpPost("detail")]
        public async Task<ActionResult<BranchDto>> GetDetail([FromBody] GetBranchByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy thông tin chi nhánh.");
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới chi nhánh (Phương thức POST)
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateBranchCommand command)
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
        /// Cập nhật chi nhánh (Phương thức POST)
        /// </summary>
        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateBranchCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy thông tin chi nhánh cần cập nhật.");
                return Ok(result);
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
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateBranchCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin chi nhánh.");
            return Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa/Ngừng hoạt động chi nhánh (Phương thức POST, sets IsDeleted = true)
        /// </summary>
        [HttpPost("deactivate")]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateBranchCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy thông tin chi nhánh.");
            return Ok(result);
        }

        /// <summary>
        /// Lấy dữ liệu rút gọn phục vụ chọn lựa SelectBox (Phương thức POST)
        /// </summary>
        [HttpPost("select-box")]
        public async Task<ActionResult<List<BranchSelectBoxDto>>> GetSelectBox([FromBody] GetBranchSelectBoxQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
