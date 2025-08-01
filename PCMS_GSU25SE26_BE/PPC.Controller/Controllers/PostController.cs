using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.PostRequest;

namespace PPC.Controller.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found.");

            var result = await _postService.CreatePostAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var result = await _postService.GetAllPostsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(string id)
        {
            var result = await _postService.GetPostByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePost([FromBody] PostUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _postService.UpdatePostAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            var result = await _postService.DeletePostAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
