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
                return ServiceResponse<string>.ErrorResponse("Tên danh mục phụ đã tồn tại trong danh mục này");
            }

            var subCategory = request.ToCreateSubCategory();
            await _subCategoryRepository.CreateAsync(subCategory);

            return ServiceResponse<string>.SuccessResponse("Danh mục phụ đã được tạo thành công");
        }

        public async Task<ServiceResponse<string>> UpdateSubCategoryAsync(SubCategoryUpdateRequest request)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(request.Id);
            if (subCategory == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy danh mục phụ");

            if (!string.Equals(subCategory.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (await _subCategoryRepository.IsNameExistInCategoryAsync(request.Name))
                {
                    return ServiceResponse<string>.ErrorResponse("Tên danh mục phụ đã tồn tại trong danh mục này");
                }

                subCategory.Name = request.Name;
            }

            subCategory.Status = request.Status;
            await _subCategoryRepository.UpdateAsync(subCategory);

            return ServiceResponse<string>.SuccessResponse("Đã cập nhật danh mục con thành công");
        }

        public async Task<ServiceResponse<string>> BlockSubCategoryAsync(string id)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(id);
            if (subCategory == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy danh mục con");

            subCategory.Status = 0; 
            var result = await _subCategoryRepository.UpdateAsync(subCategory);
            if (result != 1)
                return ServiceResponse<string>.ErrorResponse("Cập nhật trạng thái danh mục con thất bại");

            return ServiceResponse<string>.SuccessResponse("Trạng thái danh mục con đã được cập nhật thành bị chặn");
        }

        public async Task<ServiceResponse<string>> UnblockSubCategoryAsync(string id)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(id);
            if (subCategory == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy danh mục con");

            subCategory.Status = 1; 
            var result = await _subCategoryRepository.UpdateAsync(subCategory);
            if (result != 1)
                return ServiceResponse<string>.ErrorResponse("Không thể bỏ chặn danh mục con");

            return ServiceResponse<string>.SuccessResponse("Đã bỏ chặn danh mục con thành công");
        }
    }
}
