using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.PersonTypeRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CoupleResponse;
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
        private readonly IMapper _mapper;

        public ResultPersonTypeService(
            IPersonTypeRepository personTypeRepo,
            IResultPersonTypeRepository resultPersonTypeRepo,
            IMapper mapper)
        {
            _personTypeRepo = personTypeRepo;
            _resultPersonTypeRepo = resultPersonTypeRepo;
            _mapper = mapper;
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

        public async Task<ServiceResponse<List<ResultPersonTypeDto>>> GetResultByPersonTypeIdAsync(string personTypeId)
        {
            var results = await _resultPersonTypeRepo.GetByPersonTypeIdAsync(personTypeId);
            var dtos = _mapper.Map<List<ResultPersonTypeDto>>(results);

            foreach (var dto in dtos)
            {
                if (dto.PersonTypeId != personTypeId)
                {
                    var tempId = dto.PersonTypeId;
                    dto.PersonTypeId = dto.PersonType2Id;
                    dto.PersonType2Id = tempId;

                    var tempType = dto.PersonType;
                    dto.PersonType = dto.PersonType2;
                    dto.PersonType2 = tempType;
                }
            }

            return ServiceResponse<List<ResultPersonTypeDto>>.SuccessResponse(dtos);
        }

        public async Task<ServiceResponse<string>> UpdateResultPersonTypeAsync(ResultPersonTypeEditRequest request)
        {
            var result = await _resultPersonTypeRepo.GetByIdAsync(request.Id);
            if (result == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy kết quả");

            result.Description = request.Description;
            result.Detail = request.Detail;
            result.Image = request.Image;
            result.Compatibility = request.Compatibility;
            result.CategoryId = request.CategoryId;
            result.Weaknesses = request.Weaknesses;
            result.StrongPoints = request.StrongPoints;

            await _resultPersonTypeRepo.UpdateAsync(result);
            return ServiceResponse<string>.SuccessResponse("Cập nhật thành công");
        }
    }
}
