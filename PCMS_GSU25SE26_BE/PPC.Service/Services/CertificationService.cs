using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.CirtificationRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CirtificationResponse;
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



        public CertificationService(ICertificationRepository certRepo, ICounselorSubCategoryRepository cscRepo, ISubCategoryRepository subCategoryRepo, IMapper mapper, ICounselorService counselorService)
        {
            _certRepo = certRepo;
            _cscRepo = cscRepo;
            _subCategoryRepo = subCategoryRepo;
            _mapper = mapper;
            _counselorService = counselorService;
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

            return ServiceResponse<string>.SuccessResponse("Certification and subcategories submitted for approval.");
        }

        public async Task<ServiceResponse<string>> ApproveCertificationAsync(string certificationId)
        {
            var certification = await _certRepo.GetByIdAsync(certificationId);
            if (certification == null)
                return ServiceResponse<string>.ErrorResponse("Certification not found.");

            if (certification.Status == 1)
                return ServiceResponse<string>.ErrorResponse("Certification already approved.");

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

            return ServiceResponse<string>.SuccessResponse("Certification approved.");
        }

        public async Task<ServiceResponse<string>> RejectCertificationAsync(RejectCertificationRequest request)
        {
            var certification = await _certRepo.GetByIdAsync(request.CertificationId);
            if (certification == null)
                return ServiceResponse<string>.ErrorResponse("Certification not found.");

            if (certification.Status == 2)
                return ServiceResponse<string>.ErrorResponse("Certification already rejected.");

            certification.Status = 2;
            certification.RejectReason = request.RejectReason;
            await _certRepo.UpdateAsync(certification);

            var relatedSubCategories = await _cscRepo.GetByCertificationIdAsync(request.CertificationId);
            foreach (var csc in relatedSubCategories)
            {
                csc.Status = 2;
                await _cscRepo.UpdateAsync(csc);
            }

            return ServiceResponse<string>.SuccessResponse("Certification rejected with reason.");
        }

        public async Task<ServiceResponse<List<CertificationWithSubDto>>> GetMyCertificationsAsync(string counselorId)
        {
            var certifications = await _certRepo.GetByCounselorIdAsync(counselorId);
            var result = new List<CertificationWithSubDto>();

            foreach (var cert in certifications)
            {
                var dto = _mapper.Map<CertificationWithSubDto>(cert);
                var subCategories = await _cscRepo.GetSubCategoriesByCertificationIdAsync(cert.Id);
                dto.SubCategories = _mapper.Map<List<SubCategoryDto>>(subCategories);
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
                var subCategories = await _cscRepo.GetSubCategoriesByCertificationIdAsync(cert.Id);
                dto.SubCategories = _mapper.Map<List<SubCategoryDto>>(subCategories);
                result.Add(dto);
            }

            return ServiceResponse<List<CertificationWithSubDto>>.SuccessResponse(result);
        }
        public async Task<ServiceResponse<string>> UpdateCertificationAsync(string counselorId, UpdateCertificationRequest request)
        {
            var certification = await _certRepo.GetByIdAsync(request.CertificationId);
            if (certification == null)
                return ServiceResponse<string>.ErrorResponse("Certification not found.");

            if (certification.CounselorId != counselorId)
                return ServiceResponse<string>.ErrorResponse("You are not authorized to update this certification.");

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

            return ServiceResponse<string>.SuccessResponse("Certification updated and sent for re-approval.");
        }
    }
}
