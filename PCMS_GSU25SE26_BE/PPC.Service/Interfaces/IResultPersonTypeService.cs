using PPC.Service.ModelRequest.PersonTypeRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CoupleResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IResultPersonTypeService
    {
        Task<ServiceResponse<int>> GenerateAllPersonTypePairsAsync(string surveyId);
        Task<ServiceResponse<List<ResultPersonTypeDto>>> GetResultByPersonTypeIdAsync(string personTypeId);
        Task<ServiceResponse<string>> UpdateResultPersonTypeAsync(ResultPersonTypeEditRequest request);
    }

}

