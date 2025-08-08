using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.SurveyRequest;
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
        private readonly IMemberRepository _memberRepo;
        private readonly IPersonTypeRepository _personTypeRepo;
        private readonly IResultHistoryRepository _resultHistoryRepo;

        public SurveyService(ISurveyRepository surveyRepository, IMapper mapper, IMemberRepository memberRepo, IPersonTypeRepository personTypeRepo, IResultHistoryRepository resultHistoryRepo)
        {
            _surveyRepository = surveyRepository;
            _mapper = mapper;
            _memberRepo = memberRepo;
            _personTypeRepo = personTypeRepo;
            _resultHistoryRepo = resultHistoryRepo;
        }

        public async Task<ServiceResponse<List<SurveyDto>>> GetAllSurveysAsync()
        {
            var surveys = await _surveyRepository.GetAllSurveysAsync();
            var dtos = _mapper.Map<List<SurveyDto>>(surveys);
            return ServiceResponse<List<SurveyDto>>.SuccessResponse(dtos);
        }

        public async Task<ServiceResponse<string>> SubmitResultAsync(string memberId, SurveyResultRequest request)
        {
            var member = await _memberRepo.GetByIdAsync(memberId);
            if (member == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy người dùng");

            var personTypes = await _personTypeRepo.GetPersonTypesBySurveyAsync(request.SurveyId);
            var allPersonTypes = await _personTypeRepo.GetAllPersonTypesAsync();
            var description = "";
            string resultType = null;
            string categoryId = null;

            if (request.SurveyId == "SV001")
            { 
                var mbtiLetters = new List<string>();

                var eScore = request.Answers.Where(x => x.Tag == "E").Sum(x => x.Score);
                var iScore = request.Answers.Where(x => x.Tag == "I").Sum(x => x.Score);
                if (eScore + iScore == 0)
                    return ServiceResponse<string>.ErrorResponse("Missing answers for E/I dimension.");
                mbtiLetters.Add(eScore >= iScore ? "E" : "I");

                var nScore = request.Answers.Where(x => x.Tag == "N").Sum(x => x.Score);
                var sScore = request.Answers.Where(x => x.Tag == "S").Sum(x => x.Score);
                if (nScore + sScore == 0)
                    return ServiceResponse<string>.ErrorResponse("Missing answers for N/S dimension.");
                mbtiLetters.Add(nScore >= sScore ? "N" : "S");

                var tScore = request.Answers.Where(x => x.Tag == "T").Sum(x => x.Score);
                var fScore = request.Answers.Where(x => x.Tag == "F").Sum(x => x.Score);
                if (tScore + fScore == 0)
                    return ServiceResponse<string>.ErrorResponse("Missing answers for T/F dimension.");
                mbtiLetters.Add(tScore >= fScore ? "T" : "F");

                var jScore = request.Answers.Where(x => x.Tag == "J").Sum(x => x.Score);
                var pScore = request.Answers.Where(x => x.Tag == "P").Sum(x => x.Score);
                if (jScore + pScore == 0)
                    return ServiceResponse<string>.ErrorResponse("Missing answers for J/P dimension.");
                mbtiLetters.Add(jScore >= pScore ? "J" : "P");

                var type = string.Join("", mbtiLetters);

                var matched = personTypes.FirstOrDefault(pt => pt.Name == type);
                if (matched == null)
                {
                    return ServiceResponse<string>.ErrorResponse(
                        $"MBTI type [{type}] not found in system."
                    );
                }

                description = !string.IsNullOrEmpty(matched.Description)
                  ? matched.Description
                  : "Không có mô tả.";

                member.Mbti = matched.Name;
                resultType = matched.Name;
                categoryId = matched.CategoryId;
            }
            else
            {
                var highest = request.Answers
                    .OrderByDescending(a => a.Score)
                    .FirstOrDefault();

                if (highest != null)
                {
                    var matched = personTypes
                        .FirstOrDefault(pt => pt.Name == highest.Tag);

                    if (matched != null)
                    {
                        switch (request.SurveyId)
                        {
                            case "SV002": member.Disc = matched.Name; break;
                            case "SV003": member.LoveLanguage = matched.Name; break;
                            case "SV004": member.BigFive = matched.Name; break;
                        }
                        description = !string.IsNullOrEmpty(matched.Description)
                  ? matched.Description
                  : "Không có mô tả.";
                        resultType = matched.Name;
                        categoryId = matched.CategoryId;
                    }
                }
            }

            if (resultType == null)
                return ServiceResponse<string>.ErrorResponse("Không thể xác định kết quả");

            await _memberRepo.UpdateAsync(member);

            var surveysDoneRaw = new List<(string SurveyId, string Name)>
    {
        (!string.IsNullOrEmpty(member.Mbti))         ? ("SV001", member.Mbti) : default,
        (!string.IsNullOrEmpty(member.Disc))         ? ("SV002", member.Disc) : default,
        (!string.IsNullOrEmpty(member.LoveLanguage)) ? ("SV003", member.LoveLanguage) : default,
        (!string.IsNullOrEmpty(member.BigFive))      ? ("SV004", member.BigFive) : default
    }
            .Where(x => x.SurveyId != null)
            .ToList();

            var priority = new List<string> { "SV001", "SV002", "SV003", "SV004" };

            var surveysDone = surveysDoneRaw
                .OrderBy(x => priority.IndexOf(x.SurveyId))
                .ToList();

            var recs = surveysDone
                .Select(x => GetCategoryIdFromMemory(allPersonTypes, x.SurveyId, x.Name))
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();

            member.Rec1 = recs.ElementAtOrDefault(0);
            member.Rec2 = recs.ElementAtOrDefault(1);

            await _memberRepo.UpdateAsync(member);

            var detail = string.Join(",", request.Answers
    .Select(a => $"{a.Tag}:{a.Score}"));

            var history = new ResultHistory
            {
                Id = Utils.Utils.GenerateIdModel("ResultHistory"),
                MemberId = memberId,
                Type = request.SurveyId,
                Result = resultType,
                Detail = detail,
                CreateAt = Utils.Utils.GetTimeNow(),
                Description = description,
                Status = 1
            };

            await _resultHistoryRepo.CreateAsync(history);

            return ServiceResponse<string>.SuccessResponse($"Bạn thuộc kiểu {resultType} : {description}");
        }

        private string GetCategoryIdFromMemory(
            List<PersonType> allPersonTypes,
            string surveyId,
            string name)
        {
            return allPersonTypes
                .FirstOrDefault(pt =>
                    pt.SurveyId == surveyId &&
                    pt.Name == name)?.CategoryId;
        }


    }
}
