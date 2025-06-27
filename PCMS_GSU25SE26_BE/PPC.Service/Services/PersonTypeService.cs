using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.PersonTypeRequest;
using PPC.Service.ModelResponse;
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

        public PersonTypeService(IPersonTypeRepository personTypeRepository, ICategoryRepository categoryRepository, ISurveyRepository surveyRepository)
        {
            _personTypeRepository = personTypeRepository;
            _categoryRepository = categoryRepository;
            _surveyRepository = surveyRepository;
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
                Descriptione = request.Descriptione,
                Image = request.Image,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1
            };

            await _personTypeRepository.CreateAsync(personType);

            return ServiceResponse<string>.SuccessResponse("PersonType created successfully.");
        }
    }

}
