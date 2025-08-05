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
            if (await _subCategoryRepository.IsNameExistInCategoryAsync(request.Name))
            {
                return ServiceResponse<string>.ErrorResponse("Subcategory name already exists in this category.");
            }

            var subCategory = request.ToCreateSubCategory();
            await _subCategoryRepository.CreateAsync(subCategory);

            return ServiceResponse<string>.SuccessResponse("Subcategory created successfully.");
        }

        public async Task<ServiceResponse<string>> UpdateSubCategoryAsync(SubCategoryUpdateRequest request)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(request.Id);
            if (subCategory == null)
                return ServiceResponse<string>.ErrorResponse("Subcategory not found.");

            if (!string.Equals(subCategory.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (await _subCategoryRepository.IsNameExistInCategoryAsync(request.Name))
                {
                    return ServiceResponse<string>.ErrorResponse("Subcategory name already exists in this category.");
                }

                subCategory.Name = request.Name;
            }

            subCategory.Status = request.Status;
            await _subCategoryRepository.UpdateAsync(subCategory);

            return ServiceResponse<string>.SuccessResponse("Subcategory updated successfully.");
        }

        public async Task<ServiceResponse<string>> BlockSubCategoryAsync(string id)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(id);
            if (subCategory == null)
                return ServiceResponse<string>.ErrorResponse("Subcategory not found.");

            subCategory.Status = 0; 
            var result = await _subCategoryRepository.UpdateAsync(subCategory);
            if (result != 1)
                return ServiceResponse<string>.ErrorResponse("Failed to update subcategory status.");

            return ServiceResponse<string>.SuccessResponse("Subcategory status updated to blocked.");
        }

        public async Task<ServiceResponse<string>> UnblockSubCategoryAsync(string id)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(id);
            if (subCategory == null)
                return ServiceResponse<string>.ErrorResponse("Subcategory not found.");

            subCategory.Status = 1; 
            var result = await _subCategoryRepository.UpdateAsync(subCategory);
            if (result != 1)
                return ServiceResponse<string>.ErrorResponse("Failed to unblock subcategory.");

            return ServiceResponse<string>.SuccessResponse("Subcategory unblocked successfully.");
        }
    }
}
