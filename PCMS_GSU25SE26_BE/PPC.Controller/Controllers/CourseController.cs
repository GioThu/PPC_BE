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
        private readonly ICounselorService _counselorService;


        public CourseController(ICourseService courseService, IQuestionService questionService, ICounselorService counselorService)
        {
            _courseService = courseService;
            _questionService = questionService;
            _counselorService = counselorService;
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
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;

            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(memberId))
                return Unauthorized("AccountId or MemberId not found in token.");

            var response = await _courseService.GetAllCoursesAsync(accountId, memberId);
            return Ok(response);
        }

        [HttpPost("{courseId}/buy")]
        public async Task<IActionResult> EnrollCourse(string courseId)
        {
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Không tìm thấy tài khoản");

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
                return Unauthorized("Không tìm thấy người dùng");

            var response = await _courseService.GetEnrolledCoursesWithProgressAsync(memberId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "1")] 
        [HttpPost("update-course")]
        public async Task<IActionResult> UpdateCourse([FromBody] CourseUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _courseService.UpdateCourseAsync(request.CourseId, request);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [Authorize(Roles = "3")] 
        [HttpGet("{courseId}/detail-for-member")]
        public async Task<IActionResult> GetMemberCourseDetail(string courseId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("memberId not found in token.");

            var result = await _courseService.GetMemberCourseDetailAsync(courseId, memberId);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("lecture")]
        public async Task<IActionResult> UpdateLecture([FromBody] UpdateLectureRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _courseService.UpdateLectureByChapterIdAsync(request);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("video")]
        public async Task<IActionResult> UpdateVideo([FromBody] UpdateVideoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _courseService.UpdateVideoByChapterIdAsync(request);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("quiz")]
        public async Task<IActionResult> UpdateQuiz([FromBody] UpdateQuizRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _courseService.UpdateQuizByChapterIdAsync(request);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [Authorize(Roles = "3")] // Hoặc role member
        [HttpPut("enroll/{courseId}")]
        public async Task<IActionResult> OpenCourse(string courseId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Không tìm thấy thông tin thành viên.");

            var result = await _courseService.OpenCourseAsync(courseId, memberId);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("change-status")]
        public async Task<IActionResult> ChangeCourseStatus([FromBody] ChangeCourseStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.CourseId))
                return BadRequest("CourseId is required.");

            var response = await _courseService.ChangeCourseStatusAsync(request.CourseId, request.NewStatus);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
        [HttpDelete("{chapterId}")]
        public async Task<IActionResult> DeleteChapter(string chapterId)
        {
            var result = await _courseService.DeleteChapterAsync(chapterId);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("rate/{courseId}")]
        public async Task<IActionResult> RateCourse(string courseId, [FromBody] RateCourseRequest request)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Member information not found.");

            var result = await _courseService.RateCourseAsync(courseId, memberId, request.Rating, request.Feedback);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        // Endpoint to get reviews of a course
        [HttpGet("reviews/{courseId}")]
        public async Task<IActionResult> GetCourseReviews(string courseId)
        {
            var result = await _courseService.GetCourseReviewsAsync(courseId);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> RecommendCourses()
        {
            // Lấy memberId từ token
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;

            if (string.IsNullOrEmpty(memberId))
            {
                return Unauthorized("MemberId not found in token.");
            }

            // Gọi service với memberId đã lấy từ token
            var response = await _courseService.GetRecommendedCoursesAsync(memberId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }


        [HttpGet("recommendations/by-couple/{coupleId}")]
        public async Task<IActionResult> GetRecommendationsByCoupleId(string coupleId)
        {
            var response = await _courseService.GetRecommendedCoursesByCoupleIdAsync(coupleId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("max-course-discount")]
        public async Task<IActionResult> GetMaxCourseDiscount()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _courseService.GetMaxCourseDiscountByMemberWrappedAsync(memberId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
