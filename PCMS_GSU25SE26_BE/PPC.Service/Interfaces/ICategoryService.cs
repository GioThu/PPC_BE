using PPC.Service.ModelRequest.CategoryRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CategoryResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResponse<string>> CreateCategoryAsync(CategoryCreateRequest request);
        Task<ServiceResponse<string>> UpdateCategoryAsync(CategoryUpdateRequest request);
        Task<ServiceResponse<List<CategoryDto>>> GetAllCategoriesAsync();
        Task<ServiceResponse<string>> BlockCategoryAsync(string id);
        Task<ServiceResponse<string>> UnblockCategoryAsync(string id);
        Task<ServiceResponse<List<CategoryDto>>> GetActiveCategoriesWithSubAsync();


    }
}
