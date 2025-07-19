using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.CourseRequest;

namespace PPC.Controller.Controllers
{
    [Authorize(Roles = "1,2")] 
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
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
    }
}
