using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<string>> CreateCategoryAsync(CategoryCreateRequest request)
        {
            if (await _categoryRepository.IsCategoryNameExistsAsync(request.Name))
            {
                return ServiceResponse<string>.ErrorResponse("Category name already exists.");
            }

            var category = request.ToCreateCategory();
            await _categoryRepository.CreateAsync(category);

            return ServiceResponse<string>.SuccessResponse("Category created successfully.");
        }

        public async Task<ServiceResponse<string>> UpdateCategoryAsync(CategoryUpdateRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
                return ServiceResponse<string>.ErrorResponse("Category not found.");

            if (!string.IsNullOrWhiteSpace(request.Name) &&
                !string.Equals(category.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (await _categoryRepository.IsCategoryNameExistsAsync(request.Name))
                {
                    return ServiceResponse<string>.ErrorResponse("Category name already exists.");
                }

                category.Name = request.Name;
            }

            category.Status = request.Status;

            await _categoryRepository.UpdateAsync(category);
            return ServiceResponse<string>.SuccessResponse("Category updated successfully.");
        }

        public async Task<ServiceResponse<List<CategoryDto>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllWithSubCategoriesAsync();
            var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
            return ServiceResponse<List<CategoryDto>>.SuccessResponse(categoryDtos);
        }

        public async Task<ServiceResponse<string>> DeleteCategoryAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ServiceResponse<string>.ErrorResponse("Category not found.");

            var result = await _categoryRepository.RemoveAsync(category);
            if (!result)
                return ServiceResponse<string>.ErrorResponse("Failed to delete category.");

            return ServiceResponse<string>.SuccessResponse("Category deleted successfully.");
        }

        public async Task<ServiceResponse<List<CategoryDto>>> GetActiveCategoriesWithSubAsync()
        {
            var categories = await _categoryRepository.GetActiveCategoriesWithActiveSubCategoriesAsync();
            var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
            return ServiceResponse<List<CategoryDto>>.SuccessResponse(categoryDtos);
        }
    }
}
