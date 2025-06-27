using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.PersonTypeRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.PersonTypeResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class PersonTypeService : IPersonTypeService
    {
        private readonly IPersonTypeRepository _personTypeRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISurveyRepository _surveyRepository;
        private readonly IMapper _mapper;


        public PersonTypeService(IPersonTypeRepository personTypeRepository, ICategoryRepository categoryRepository, ISurveyRepository surveyRepository, IMapper mapper)
        {
            _personTypeRepository = personTypeRepository;
            _categoryRepository = categoryRepository;
            _surveyRepository = surveyRepository;
            _mapper = mapper;

        }

        public async Task<ServiceResponse<string>> CreatePersonTypeAsync(CreatePersonTypeRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                return ServiceResponse<string>.ErrorResponse("Category not found.");

            var survey = await _surveyRepository.GetByIdAsync(request.SurveyId);
            if (survey == null)
                return ServiceResponse<string>.ErrorResponse("Survey not found.");

            var personType = new PersonType
            {
                Id = Utils.Utils.GenerateIdModel("PersonType"),
                CategoryId = request.CategoryId,
                SurveyId = request.SurveyId,
                Name = request.Name,
                Description = request.Description,
                Image = request.Image,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1
            };

            await _personTypeRepository.CreateAsync(personType);

            return ServiceResponse<string>.SuccessResponse("PersonType created successfully.");
        }

        public async Task<ServiceResponse<List<PersonTypeDto>>> GetPersonTypesBySurveyAsync(string surveyId)
        {
            var list = await _personTypeRepository.GetPersonTypesBySurveyAsync(surveyId);
            var dtos = _mapper.Map<List<PersonTypeDto>>(list);
            return ServiceResponse<List<PersonTypeDto>>.SuccessResponse(dtos);
        }

        public async Task<ServiceResponse<PersonTypeDto>> GetPersonTypeByIdAsync(string id)
        {
            var pt = await _personTypeRepository.GetPersonTypeByIdAsync(id);
            if (pt == null)
                return ServiceResponse<PersonTypeDto>.ErrorResponse("PersonType not found");

            var dto = _mapper.Map<PersonTypeDto>(pt);
            return ServiceResponse<PersonTypeDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<string>> UpdatePersonTypeAsync(PersonTypeUpdateRequest request)
        {
            var entity = await _personTypeRepository.GetPersonTypeByIdAsync(request.Id);
            if (entity == null)
                return ServiceResponse<string>.ErrorResponse("PersonType not found");

            entity.Description = request.Description;
            entity.Detail = request.Detail;
            entity.Image = request.Image;
            entity.CategoryId = request.CategoryId;

            await _personTypeRepository.UpdatePersonTypeAsync(entity);
            return ServiceResponse<string>.SuccessResponse("Update successful");
        }
    }

}
