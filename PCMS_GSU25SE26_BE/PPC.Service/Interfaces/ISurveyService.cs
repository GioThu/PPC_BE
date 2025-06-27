using PPC.Service.ModelRequest.SurveyRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.SurveyResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ISurveyService
    {
        Task<ServiceResponse<List<SurveyDto>>> GetAllSurveysAsync();
        Task<ServiceResponse<string>> SubmitResultAsync(string memberId, SurveyResultRequest request);

    }
}
