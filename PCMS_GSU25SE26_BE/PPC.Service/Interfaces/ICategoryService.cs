using PPC.Service.ModelRequest;
using PPC.Service.ModelResponse;
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
        Task<ServiceResponse<string>> DeleteCategoryAsync(string id);
        Task<ServiceResponse<List<CategoryDto>>> GetActiveCategoriesWithSubAsync();


    }
}
