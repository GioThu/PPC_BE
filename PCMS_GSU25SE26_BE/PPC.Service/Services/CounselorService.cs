using AutoMapper;
using PPC.Repository.Interfaces;
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

        public CounselorService(ICounselorRepository counselorRepository, IMapper mapper)
        {
            _counselorRepository = counselorRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<CounselorDto>>> GetAllCounselorsAsync()
        {
            var counselors = await _counselorRepository.GetAllAsync();
            var counselorDtos = _mapper.Map<List<CounselorDto>>(counselors);
            return ServiceResponse<List<CounselorDto>>.SuccessResponse(counselorDtos);
        }
    }
}
