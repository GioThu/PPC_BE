using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.CirtificationRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CirtificationResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class CertificationService : ICertificationService
    {
        private readonly ICertificationRepository _certRepo;
        private readonly ICounselorSubCategoryRepository _cscRepo;
        private readonly ISubCategoryRepository _subCategoryRepo;
        private readonly IMapper _mapper;
        private readonly ICounselorService _counselorService;
        private readonly ICounselorRepository _counselorRepo;



        public CertificationService(ICertificationRepository certRepo, ICounselorSubCategoryRepository cscRepo, ISubCategoryRepository subCategoryRepo, IMapper mapper, ICounselorService counselorService, ICounselorRepository counselorRepo)
        {
            _certRepo = certRepo;
            _cscRepo = cscRepo;
            _subCategoryRepo = subCategoryRepo;
            _mapper = mapper;
            _counselorService = counselorService;
            _counselorRepo = counselorRepo;
        }

        public async Task<ServiceResponse<string>> SendCertificationAsync(string counselorId, SendCertificationRequest request)
        {
            // Tạo chứng chỉ
            var cert = request.ToCertification(counselorId);
            await _certRepo.CreateAsync(cert);

            // Lấy danh sách sub category để lấy CategoryId
            var subCategories = await _subCategoryRepo.GetByIdsAsync(request.SubCategoryIds);

            var links = subCategories.Select(sc => new CounselorSubCategory
            {
                Id = Utils.Utils.GenerateIdModel("CounselorSubCategory"),
                CounselorId = counselorId,
                CertifivationId = cert.Id,
                SubCategoryId = sc.Id,
                CategoryId = sc.CategoryId,
                Status = 0
            }).ToList();

            foreach (var link in links)
            {
                await _cscRepo.CreateAsync(link);
            }

            return ServiceResponse<string>.SuccessResponse("Chứng chỉ và các chuyên môn phụ đã được gửi để phê duyệt");
        }

        public async Task<ServiceResponse<string>> ApproveCertificationAsync(string certificationId)
        {
            var certification = await _certRepo.GetByIdAsync(certificationId);
            if (certification == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy chứng chỉ");

            if (certification.Status == 1)
                return ServiceResponse<string>.ErrorResponse("Chứng chỉ đã được phê duyệt");

            certification.Status = 1;
            certification.RejectReason = null;

            await _certRepo.UpdateAsync(certification);

            var relatedSubCategories = await _cscRepo.GetByCertificationIdAsync(certificationId);
            foreach (var csc in relatedSubCategories)
            {
                csc.Status = 1;
                await _cscRepo.UpdateAsync(csc);
            }
            await _counselorService.CheckAndUpdateCounselorStatusAsync(certification.CounselorId);

            return ServiceResponse<string>.SuccessResponse("Chứng chỉ đã được phê duyệt");
        }

        public async Task<ServiceResponse<string>> RejectCertificationAsync(RejectCertificationRequest request)
        {
            var certification = await _certRepo.GetByIdAsync(request.CertificationId);
            if (certification == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy chứng chỉ");

            if (certification.Status == 2)
                return ServiceResponse<string>.ErrorResponse("Chứng chỉ đã bị từ chối");

            certification.Status = 2;
            certification.RejectReason = request.RejectReason;
            await _certRepo.UpdateAsync(certification);

            var relatedSubCategories = await _cscRepo.GetByCertificationIdAsync(request.CertificationId);
            foreach (var csc in relatedSubCategories)
            {
                csc.Status = 2;
                await _cscRepo.UpdateAsync(csc);
            }

            return ServiceResponse<string>.SuccessResponse("Đã từ chối chứng chỉ");
        }

        public async Task<ServiceResponse<List<CertificationWithSubDto>>> GetMyCertificationsAsync(string counselorId)
        {
            var certifications = await _certRepo.GetByCounselorIdAsync(counselorId);
            var result = new List<CertificationWithSubDto>();

            foreach (var cert in certifications)
            {
                var dto = _mapper.Map<CertificationWithSubDto>(cert);

                dto.Counselor = _mapper.Map<CounselorDto>(cert.Counselor);

                var subCategories = await _cscRepo.GetSubCategoriesByCertificationIdAsync(cert.Id);

                if (subCategories == null || !subCategories.Any())
                {
                    dto.Categories = new List<CategoryWithSubDto>(); 
                }
                else
                {
                    var categories = subCategories
                        .Where(sc => sc.Category != null) 
                        .GroupBy(sc => sc.CategoryId)
                        .Select(group => new CategoryWithSubDto
                        {
                            CategoryId = group.Key,
                            CategoryName = group.First().Category?.Name ?? "Vô danh", 
                            SubCategories = _mapper.Map<List<SubCategoryDto>>(group.ToList())
                        })
                        .ToList();

                    dto.Categories = categories;
                }

                result.Add(dto);
            }

            return ServiceResponse<List<CertificationWithSubDto>>.SuccessResponse(result);
        }

        public async Task<ServiceResponse<List<CertificationWithSubDto>>> GetAllCertificationsAsync()
        {
            var certifications = await _certRepo.GetAllCertificationsAsync();
            var result = new List<CertificationWithSubDto>();

            foreach (var cert in certifications)
            {
                var dto = _mapper.Map<CertificationWithSubDto>(cert);

                dto.Counselor = _mapper.Map<CounselorDto>(cert.Counselor);
                var subCategories = await _cscRepo.GetSubCategoriesByCertificationIdAsync(cert.Id);

                var categories = subCategories
                    .Where(sc => sc.Category != null) 
                    .GroupBy(sc => sc.CategoryId)
                    .Select(group => new CategoryWithSubDto
                    {
                        CategoryId = group.Key,
                        CategoryName = group.First().Category.Name, 
                        SubCategories = _mapper.Map<List<SubCategoryDto>>(group.ToList()) 
                    })
                    .ToList();

                dto.Categories = categories;
                result.Add(dto); 
            }

            return ServiceResponse<List<CertificationWithSubDto>>.SuccessResponse(result);
        }

        public async Task<ServiceResponse<CertificationWithSubDto>> GetCertificationByIdAsync(string certificationId)
        {
            var certification = await _certRepo.GetCertificationByIdAsync(certificationId);

            if (certification == null)
            {
                return ServiceResponse<CertificationWithSubDto>.ErrorResponse("Không tìm thấy chứng chỉ");
            }

            var dto = _mapper.Map<CertificationWithSubDto>(certification);

            dto.Counselor = _mapper.Map<CounselorDto>(certification.Counselor);

            var subCategories = await _cscRepo.GetSubCategoriesByCertificationIdAsync(certificationId);

            var categories = subCategories
                .Where(sc => sc.Category != null) 
                .GroupBy(sc => sc.CategoryId)
                .Select(group => new CategoryWithSubDto
                {
                    CategoryId = group.Key,
                    CategoryName = group.First().Category.Name,
                    SubCategories = _mapper.Map<List<SubCategoryDto>>(group.ToList()) 
                })
                .ToList();

            dto.Categories = categories;

            return ServiceResponse<CertificationWithSubDto>.SuccessResponse(dto);
        }
        public async Task<ServiceResponse<string>> UpdateCertificationAsync(string counselorId, UpdateCertificationRequest request)
        {
            var certification = await _certRepo.GetByIdAsync(request.CertificationId);
            if (certification == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy chứng chỉ");

            if (certification.CounselorId != counselorId)
                return ServiceResponse<string>.ErrorResponse("Bạn không có quyền cập nhật chứng chỉ này");

            certification.Name = request.Name;
            certification.Image = request.Image;
            certification.Description = request.Description;
            certification.Time = request.Time;
            certification.Status = 0;
            certification.RejectReason = null;

            await _certRepo.UpdateAsync(certification);

            await _cscRepo.RemoveByCertificationIdAsync(certification.Id);

            var subCategories = await _subCategoryRepo.GetByIdsAsync(request.SubCategoryIds);

            var newLinks = subCategories.Select(sc => new CounselorSubCategory
            {
                Id = Utils.Utils.GenerateIdModel("CounselorSubCategory"),
                CounselorId = counselorId,
                CertifivationId = certification.Id,
                SubCategoryId = sc.Id,
                CategoryId = sc.CategoryId,
                Status = 0
            }).ToList();

            foreach (var link in newLinks)
            {
                await _cscRepo.CreateAsync(link);
            }
            await _counselorService.CheckAndUpdateCounselorStatusAsync(certification.CounselorId);

            return ServiceResponse<string>.SuccessResponse("Chứng chỉ đã được cập nhật và gửi lại để phê duyệt");
        }
        public async Task<ServiceResponse<PagingResponse<CertificationWithSubDto>>> GetAllCertificationsAsync(int pageNumber, int pageSize, int? status)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10; 

            var certifications = await _certRepo.GetAllCertificationsAsync();

            if (status.HasValue)
            {
                certifications = certifications
                    .Where(c => c.Counselor != null && c.Counselor.Status == status.Value)
                    .ToList();
            }

            var totalCount = certifications.Count();

            // Áp dụng phân trang
            var certificationsPaged = certifications
                .Skip((pageNumber - 1) * pageSize)  
                .Take(pageSize) 
                .ToList();

            var result = new List<CertificationWithSubDto>();
            foreach (var cert in certificationsPaged)
            {
                var dto = _mapper.Map<CertificationWithSubDto>(cert);
                dto.Counselor = _mapper.Map<CounselorDto>(cert.Counselor);

                var subCategories = await _cscRepo.GetSubCategoriesByCertificationIdAsync(cert.Id);
                var categories = subCategories
                    .Where(sc => sc.Category != null)
                    .GroupBy(sc => sc.CategoryId)
                    .Select(group => new CategoryWithSubDto
                    {
                        CategoryId = group.Key,
                        CategoryName = group.First().Category.Name,
                        SubCategories = _mapper.Map<List<SubCategoryDto>>(group.ToList())
                    })
                    .ToList();

                dto.Categories = categories;
                result.Add(dto);
            }

            var pagingResponse = new PagingResponse<CertificationWithSubDto>(result, totalCount, pageNumber, pageSize);

            return ServiceResponse<PagingResponse<CertificationWithSubDto>>.SuccessResponse(pagingResponse);
        }

        public async Task<bool> IsCertificationAssignedToCounselorAsync(string certificationId, string counselorId)
        {
            var certification = await _certRepo.GetByIdAsync(certificationId);
            if (certification == null)
            {
                return false;
            }
            var isAssigned = false;
            if (certification.CounselorId == counselorId)
            {
                isAssigned = true;
            }
            return isAssigned;
        }

    }
}
