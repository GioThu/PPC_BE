using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.CategoryRequest;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryService _subCategoryService;

        public SubCategoryController(ISubCategoryService subCategoryService)
        {
            _subCategoryService = subCategoryService;
        }

        // Tạo SubCategory mới
        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<IActionResult> CreateSubCategory([FromBody] SubCategoryCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _subCategoryService.CreateSubCategoryAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        // Cập nhật SubCategory
        [Authorize(Roles = "1")]
        [HttpPut]
        public async Task<IActionResult> UpdateSubCategory([FromBody] SubCategoryUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _subCategoryService.UpdateSubCategoryAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

    }
}
