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
        private readonly IMemberRepository _memberRepo;
        private readonly IResultHistoryRepository _resultHistoryRepo;
        private readonly IBookingRepository _bookingRepo;


        public PersonTypeService(IPersonTypeRepository personTypeRepository, ICategoryRepository categoryRepository, ISurveyRepository surveyRepository, IMapper mapper, IMemberRepository memberRepo, IResultHistoryRepository resultHistoryRepo, IBookingRepository bookingRepo)
        {
            _personTypeRepository = personTypeRepository;
            _categoryRepository = categoryRepository;
            _surveyRepository = surveyRepository;
            _mapper = mapper;
            _memberRepo = memberRepo;
            _resultHistoryRepo = resultHistoryRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<ServiceResponse<string>> CreatePersonTypeAsync(CreatePersonTypeRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy danh mục");

            var survey = await _surveyRepository.GetByIdAsync(request.SurveyId);
            if (survey == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy khảo sát");

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

            return ServiceResponse<string>.SuccessResponse("Kiểu người đã được tạo thành công");
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
                return ServiceResponse<PersonTypeDto>.ErrorResponse("Không tìm thấy kiểu người");

            var dto = _mapper.Map<PersonTypeDto>(pt);
            return ServiceResponse<PersonTypeDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<string>> UpdatePersonTypeAsync(PersonTypeUpdateRequest request)
        {
            var entity = await _personTypeRepository.GetPersonTypeByIdAsync(request.Id);
            if (entity == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy kiểu người");

            entity.Description = request.Description;
            entity.Detail = request.Detail;
            entity.Image = request.Image;
            entity.CategoryId = request.CategoryId;

            await _personTypeRepository.UpdatePersonTypeAsync(entity);
            return ServiceResponse<string>.SuccessResponse("Cập nhật thành công");
        }

        public async Task<ServiceResponse<MyPersonTypeResponse>> GetMyPersonTypeAsync(string memberId, string surveyId)
        {
            // Lấy Member
            var member = await _memberRepo.GetByIdAsync(memberId);
            if (member == null)
                return ServiceResponse<MyPersonTypeResponse>.ErrorResponse("Không tìm thấy người dùng");

            // Xác định name theo surveyId
            string personTypeName = surveyId switch
            {
                "SV001" => member.Mbti,
                "SV002" => member.Disc,
                "SV003" => member.LoveLanguage,
                "SV004" => member.BigFive,
                _ => null
            };

            if (string.IsNullOrEmpty(personTypeName))
                return ServiceResponse<MyPersonTypeResponse>.ErrorResponse("Không tìm thấy kiểu người nào cho khảo sát này");

            // Tìm trong PersonType table
            var personType = await _personTypeRepository
                .GetPersonTypesBySurveyAsync(surveyId);

            var matched = personType
                .FirstOrDefault(pt => pt.Name == personTypeName);

            if (matched == null)
                return ServiceResponse<MyPersonTypeResponse>.ErrorResponse("Không tìm thấy kiểu người trong hệ thống");

            // Map sang DTO
            var dto = _mapper.Map<MyPersonTypeResponse>(matched);
            var history = await _resultHistoryRepo.GetLatestResultAsync(memberId, surveyId);
            if (history != null && !string.IsNullOrEmpty(history.Detail))
            {
                dto.Scores = history.Detail
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(pair => pair.Split(':'))
                    .Where(parts => parts.Length == 2)
                    .ToDictionary(parts => parts[0], parts => int.Parse(parts[1]));
            }

            return ServiceResponse<MyPersonTypeResponse>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<List<ResultHistoryResponse>>> GetHistoryByMemberAndSurveyAsync(string memberId, string surveyId)
        {
            var histories = await _resultHistoryRepo.GetResultHistoriesByMemberAndSurveyAsync(memberId, surveyId);

            if (!histories.Any())
                return ServiceResponse<List<ResultHistoryResponse>>.ErrorResponse("Không tìm thấy lịch sử tư vấn");

            var responses = new List<ResultHistoryResponse>();

            foreach (var history in histories)
            {

                var scores = new Dictionary<string, int>();
                if (!string.IsNullOrEmpty(history.Detail))
                {
                    scores = history.Detail
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Split(':'))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(
                            parts => parts[0],
                            parts => int.Parse(parts[1])
                        );
                }

                var dto = new ResultHistoryResponse
                {
                    SurveyId = history.Type,
                    Result = history.Result,
                    Description = history.Description,
                    RawScores = history.Detail,
                    Scores = scores,
                    CreateAt = history.CreateAt
                };

                responses.Add(dto);
            }

            return ServiceResponse<List<ResultHistoryResponse>>.SuccessResponse(responses);
        }

        public async Task<ServiceResponse<PersonTypeDto>> GetByNameAndSurveyIdAsync(PersonTypeByNameRequest request)
        {
            var personType = await _personTypeRepository.GetByNameAndSurveyIdAsync(request.Name, request.SurveyId);
            if (personType == null)
                return ServiceResponse<PersonTypeDto>.ErrorResponse("Không tìm thấy kiểu người");

            var dto = _mapper.Map<PersonTypeDto>(personType);
            return ServiceResponse<PersonTypeDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<List<ResultHistoryResponse>>> GetHistoryByMemberAndSurveyAsync(string memberId, string surveyId, string bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                return ServiceResponse<List<ResultHistoryResponse>>.ErrorResponse("Không tìm thấy lịch hẹn");

            if (!booking.TimeStart.HasValue)
                return ServiceResponse<List<ResultHistoryResponse>>.ErrorResponse("Lịch hẹn chưa có thời gian bắt đầu");

            var cutoff = booking.TimeStart.Value;

            var histories = await _resultHistoryRepo.GetResultHistoriesByMemberAndSurveyAsync(memberId, surveyId);

            histories = histories
                .Where(h => h.CreateAt.HasValue && h.CreateAt.Value <= cutoff)
                .OrderBy(h => h.CreateAt!.Value)
                .ToList();

            if (!histories.Any())
                return ServiceResponse<List<ResultHistoryResponse>>.ErrorResponse("Không có lịch sử trước thời điểm bắt đầu lịch hẹn");

            var responses = new List<ResultHistoryResponse>();

            foreach (var history in histories)
            {
                var scores = new Dictionary<string, int>();
                if (!string.IsNullOrEmpty(history.Detail))
                {
                    scores = history.Detail
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Split(':'))
                        .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
                        .ToDictionary(
                            parts => parts[0],
                            parts => int.Parse(parts[1])
                        );
                }

                var dto = new ResultHistoryResponse
                {
                    SurveyId = history.Type,
                    Result = history.Result,
                    Description = history.Description,
                    RawScores = history.Detail,
                    Scores = scores,
                    CreateAt = history.CreateAt
                };

                responses.Add(dto);
            }

            return ServiceResponse<List<ResultHistoryResponse>>.SuccessResponse(responses);
        }
    }

}
