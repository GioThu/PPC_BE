using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.SurveyResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IMapper _mapper;

        public SurveyService(ISurveyRepository surveyRepository, IMapper mapper)
        {
            _surveyRepository = surveyRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<SurveyDto>>> GetAllSurveysAsync()
        {
            var surveys = await _surveyRepository.GetAllSurveysAsync();
            var dtos = _mapper.Map<List<SurveyDto>>(surveys);
            return ServiceResponse<List<SurveyDto>>.SuccessResponse(dtos);
        }
    }
}
