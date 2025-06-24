using PPC.Service.ModelRequest.SurveyRequest;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IQuestionService
    {
        Task<ServiceResponse<string>> CreateQuestionAsync(SurveyQuestionCreateRequest request);

        Task<ServiceResponse<PagingResponse<SurveyQuestionDto>>> GetPagingAsync(PagingSurveyQuestionRequest request);
        Task<ServiceResponse<string>> UpdateAsync(string questionId, SurveyQuestionUpdateRequest request);
        Task<ServiceResponse<string>> DeleteAsync(string questionId);
    }
}
