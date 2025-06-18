using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.CategoryRequest;

namespace PPC.Controller.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _categoryService.CreateCategoryAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _categoryService.UpdateCategoryAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var response = await _categoryService.GetAllCategoriesAsync();
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }


        [HttpGet("active-with-sub")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetActiveCategoriesWithSub()
        {
            var response = await _categoryService.GetActiveCategoriesWithSubAsync();
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
