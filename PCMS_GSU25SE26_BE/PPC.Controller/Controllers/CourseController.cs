using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.CourseRequest;
using PPC.Service.ModelRequest.SurveyRequest;
using PPC.Service.Services;

namespace PPC.Controller.Controllers
{
    [Authorize(Roles = "1,2,3")] 
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IQuestionService _questionService;


        public CourseController(ICourseService courseService, IQuestionService questionService)
        {
            _courseService = courseService;
            _questionService = questionService;

        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creatorId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(creatorId))
                return Unauthorized("User ID not found in token.");

            var response = await _courseService.CreateCourseAsync(creatorId, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var response = await _courseService.GetAllCoursesAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("add-subcate")]
        public async Task<IActionResult> AddSubCategory([FromBody] CourseSubCategoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _courseService.AddSubCategoryAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("remove-subcate")]
        public async Task<IActionResult> RemoveSubCategory([FromBody] CourseSubCategoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _courseService.RemoveSubCategoryAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("create-lecture")]
        public async Task<IActionResult> CreateLectureWithChapter([FromBody] LectureWithChapterCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _courseService.CreateLectureWithChapterAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("create-video")]
        public async Task<IActionResult> CreateVideoWithChapter([FromBody] VideoWithChapterCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _courseService.CreateVideoWithChapterAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("create-quiz")]
        public async Task<IActionResult> CreateQuizWithChapter([FromBody] QuizWithChapterCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _courseService.CreateQuizWithChapterAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourseDetail(string courseId)
        {
            var response = await _courseService.GetCourseDetailByIdAsync(courseId);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpGet("{id}/chapter-detail")]
        public async Task<IActionResult> GetChapterDetail(string id)
        {
            var response = await _courseService.GetChapterDetailAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpGet("by-quiz/{quizId}")]
        public async Task<IActionResult> GetByQuizId(string quizId)
        {
            var result = await _questionService.GetQuestionsByQuizIdAsync(quizId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "1")]
        [HttpPost("create-question")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _questionService.CreateQuestion1Async(request);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [Authorize(Roles = "1")]
        [HttpDelete("delete-question/{questionId}")]
        public async Task<IActionResult> Delete(string questionId)
        {
            var result = await _questionService.DeleteAsync(questionId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "1")]
        [HttpPut("update-question")]
        public async Task<IActionResult> Update([FromBody] QuestionUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _questionService.UpdateAsync(request.Id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all-for-user")]
        public async Task<IActionResult> GetAllCoursesByUsers()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Account ID not found in token");

            var result = await _courseService.GetAllCoursesAsync(memberId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{courseId}/enroll")]
        public async Task<IActionResult> EnrollCourse(string courseId)
        {
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Account not found.");

            var result = await _courseService.EnrollCourseAsync(courseId, accountId);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Member not found.");

            var response = await _courseService.GetEnrolledCoursesWithProgressAsync(memberId);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
