using PPC.DAO.Models;
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
    public class ResultPersonTypeService : IResultPersonTypeService
    {
        private readonly IPersonTypeRepository _personTypeRepo;
        private readonly IResultPersonTypeRepository _resultPersonTypeRepo;

        public ResultPersonTypeService(
            IPersonTypeRepository personTypeRepo,
            IResultPersonTypeRepository resultPersonTypeRepo)
        {
            _personTypeRepo = personTypeRepo;
            _resultPersonTypeRepo = resultPersonTypeRepo;
        }

        public async Task<ServiceResponse<int>> GenerateAllPersonTypePairsAsync(string surveyId)
        {
            var personTypes = await _personTypeRepo.GetPersonTypesBySurveyAsync(surveyId);

            if (personTypes == null || !personTypes.Any())
            {
                return ServiceResponse<int>.ErrorResponse("No PersonTypes found for this survey.");
            }

            var resultPairs = new List<ResultPersonType>();

            for (int i = 0; i < personTypes.Count; i++)
            {
                var p1 = personTypes[i];

                for (int j = i; j < personTypes.Count; j++)
                {
                    var p2 = personTypes[j];

                    var result = new ResultPersonType
                    {
                        Id = Utils.Utils.GenerateIdModel("ResultPersonType"),
                        SurveyId = surveyId,
                        PersonTypeId = p1.Id,
                        PersonType2Id = p2.Id,
                        CategoryId = p1.CategoryId, // hoặc logic bạn muốn
                        Description = null,
                        Detail = null,
                        Compatibility = 0,
                        Image = null,
                        CreateAt = Utils.Utils.GetTimeNow(),
                        Status = 1
                    };

                    resultPairs.Add(result);
                }
            }

            await _resultPersonTypeRepo.BulkInsertAsync(resultPairs);

            return ServiceResponse<int>.SuccessResponse(resultPairs.Count);
        }
    }
}
