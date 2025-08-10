using PPC.Service.ModelRequest.PersonTypeRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.PersonTypeResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IPersonTypeService
    {
        Task<ServiceResponse<string>> CreatePersonTypeAsync(CreatePersonTypeRequest request);
        Task<ServiceResponse<List<PersonTypeDto>>> GetPersonTypesBySurveyAsync(string surveyId);
        Task<ServiceResponse<PersonTypeDto>> GetPersonTypeByIdAsync(string id);
        Task<ServiceResponse<string>> UpdatePersonTypeAsync(PersonTypeUpdateRequest request);
        Task<ServiceResponse<MyPersonTypeResponse>> GetMyPersonTypeAsync(string memberId, string surveyId);
        Task<ServiceResponse<List<ResultHistoryResponse>>> GetHistoryByMemberAndSurveyAsync(string memberId, string surveyId);
        Task<ServiceResponse<PersonTypeDto>> GetByNameAndSurveyIdAsync(PersonTypeByNameRequest request);
        Task<ServiceResponse<List<ResultHistoryResponse>>> GetHistoryByMemberAndSurveyAsync(string memberId, string surveyId, string bookingId);

    }
}
