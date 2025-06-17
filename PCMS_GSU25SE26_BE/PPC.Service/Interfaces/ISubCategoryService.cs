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
    public interface ISubCategoryService
    {
        Task<ServiceResponse<string>> CreateSubCategoryAsync(SubCategoryCreateRequest request);
        Task<ServiceResponse<string>> UpdateSubCategoryAsync(SubCategoryUpdateRequest request);
        Task<ServiceResponse<string>> BlockSubCategoryAsync(string id);
        Task<ServiceResponse<string>> UnblockSubCategoryAsync(string id);
    }
}
