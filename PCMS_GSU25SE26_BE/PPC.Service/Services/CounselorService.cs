using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class CounselorService : ICounselorService
    {
        private readonly ICounselorRepository _counselorRepository;
        private readonly IMapper _mapper;
        private readonly ICounselorSubCategoryRepository _counselorSubCategoryRepository;

        public CounselorService(ICounselorRepository counselorRepository, IMapper mapper, ICounselorSubCategoryRepository counselorSubCategoryRepository)
        {
            _counselorRepository = counselorRepository;
            _mapper = mapper;
            _counselorSubCategoryRepository = counselorSubCategoryRepository;
        }

        public async Task<ServiceResponse<List<CounselorDto>>> GetAllCounselorsAsync()
        {
            var counselors = await _counselorRepository.GetAllAsync();
            var counselorDtos = _mapper.Map<List<CounselorDto>>(counselors);
            return ServiceResponse<List<CounselorDto>>.SuccessResponse(counselorDtos);
        }

        public async Task CheckAndUpdateCounselorStatusAsync(string counselorId)
        {
            var counselor = await _counselorRepository.GetByIdAsync(counselorId);
            if (counselor == null) return;

            var hasApproved = await _counselorSubCategoryRepository
                .HasAnyApprovedSubCategoryAsync(counselorId);

            var newStatus = hasApproved ? 1 : 0;

            if (counselor.Status != newStatus)
            {
                counselor.Status = newStatus;
                await _counselorRepository.UpdateAsync(counselor);
            }
        }
    }
}
