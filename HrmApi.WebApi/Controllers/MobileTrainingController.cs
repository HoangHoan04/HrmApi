using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.DTOs.Training;
using HrmApi.Application.Features.Mobile;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [RequirePermission(PermissionCodes.MobileAccess)]
    [Route("api/v1/mobile/training")]
    public class MobileTrainingController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileTrainingController(IMediator mediator) => _mediator = mediator;

        [HttpPost("my-courses")]
        public async Task<ActionResult<List<TrainingCourseDto>>> MyCourses([FromBody] GetMyTrainingCoursesQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyTrainingCoursesQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("my-enrollments")]
        public async Task<ActionResult<List<TrainingEnrollmentDto>>> MyEnrollments([FromBody] GetMyTrainingEnrollmentsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyTrainingEnrollmentsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("my-results")]
        public async Task<ActionResult<List<TrainingResultDto>>> MyResults([FromBody] GetMyTrainingResultsQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyTrainingResultsQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("quizzes")]
        public async Task<ActionResult<List<MobileQuizQuestionDto>>> Quizzes([FromBody] GetMobileQuizzesQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("submit-quiz")]
        public async Task<ActionResult<MobileSubmitQuizResultDto>> SubmitQuiz([FromBody] SubmitMobileQuizCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
