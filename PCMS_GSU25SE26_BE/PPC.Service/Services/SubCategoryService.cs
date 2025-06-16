using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.CategoryRequest;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly ISubCategoryRepository _subCategoryRepository;

        public SubCategoryService(ISubCategoryRepository subCategoryRepository)
        {
            _subCategoryRepository = subCategoryRepository;
        }

        public async Task<ServiceResponse<string>> CreateSubCategoryAsync(SubCategoryCreateRequest request)
        {
            if (await _subCategoryRepository.IsNameExistInCategoryAsync(request.Name, request.CategoryId))
            {
                return ServiceResponse<string>.ErrorResponse("Subcategory name already exists in this category.");
            }

            var subCategory = request.ToCreateSubCategory();
            await _subCategoryRepository.CreateAsync(subCategory);

            return ServiceResponse<string>.SuccessResponse("Subcategory created successfully.");
        }
    }
}
