using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.Couple;
using PPC.Service.ModelRequest.SurveyRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.Couple;
using PPC.Service.ModelResponse.CoupleResponse;
using PPC.Service.ModelResponse.MemberResponse;
using PPC.Service.Utils;

public class CoupleService : ICoupleService
{
    private readonly ICoupleRepository _coupleRepository;
    private readonly IMapper _mapper;
    private readonly IMemberRepository _memberRepo;
    private readonly IPersonTypeRepository _personTypeRepo;
    private readonly IResultPersonTypeRepository _resultPersonTypeRepo;


    public CoupleService(ICoupleRepository coupleRepository, IMapper mapper, IMemberRepository memberRepo, IPersonTypeRepository personTypeRepo, IResultPersonTypeRepository resultPersonTypeRepo)
    {
        _coupleRepository = coupleRepository;
        _mapper = mapper;
        _memberRepo = memberRepo;
        _personTypeRepo = personTypeRepo;
        _resultPersonTypeRepo = resultPersonTypeRepo;
    }

    public async Task<ServiceResponse<string>> JoinCoupleByAccessCodeAsync(string memberId, string accessCode)
    {
        var hasActive = await _coupleRepository.HasActiveCoupleAsync(memberId);
        if (hasActive)
        {
            return ServiceResponse<string>.ErrorResponse("You already have an active room. Cannot join another.");
        }

        var couple = await _coupleRepository.GetByAccessCodeAsync(accessCode);
        if (couple == null)
            return ServiceResponse<string>.ErrorResponse("Room not found.");

        if (couple.IsVirtual == true)
            return ServiceResponse<string>.ErrorResponse("This is a virtual room. Cannot join.");

        if (couple.Member == memberId)
            return ServiceResponse<string>.ErrorResponse("You cannot join your own room.");

        if (!string.IsNullOrEmpty(couple.Member1))
            return ServiceResponse<string>.ErrorResponse("This room already has a partner.");

        var ownerLatestCouple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(couple.Member);
        if (ownerLatestCouple == null || ownerLatestCouple.Id != couple.Id || ownerLatestCouple.Status != 1)
        {
            return ServiceResponse<string>.ErrorResponse("This room is not the latest active room of the owner. Cannot join.");
        }

        couple.Member1 = memberId;
        await _coupleRepository.UpdateAsync(couple);

        return ServiceResponse<string>.SuccessResponse("Joined room successfully.");
    }

    public async Task<ServiceResponse<CoupleDetailResponse>> GetCoupleDetailAsync(string coupleId)
    {
        var couple = await _coupleRepository.GetCoupleByIdWithMembersAsync(coupleId);

        if (couple == null)
            return ServiceResponse<CoupleDetailResponse>.ErrorResponse("Couple not found.");

        var dto = _mapper.Map<CoupleDetailResponse>(couple);

        return ServiceResponse<CoupleDetailResponse>.SuccessResponse(dto);
    }

    public async Task<ServiceResponse<string>> CreateCoupleAsync(string memberId, CoupleCreateRequest request)
    {
        var hasActive = await _coupleRepository.HasActiveCoupleAsync(memberId);
        if (hasActive)
        {
            return ServiceResponse<string>.ErrorResponse("You already have an active room. Cannot create a new one.");
        }

        var couple = new Couple
        {
            Id = Utils.GenerateIdModel("Couple"),
            Member = memberId,
            AccessCode = Utils.GenerateAccessCode(),
            CreateAt = Utils.GetTimeNow(),
            Status = 1
        };

        if (request.SurveyIds != null)
        {
            foreach (var sv in request.SurveyIds)
            {
                switch (sv)
                {
                    case "SV001":
                        couple.Mbti = "false";
                        couple.Mbti1 = "false";
                        break;
                    case "SV002":
                        couple.Disc = "false";
                        couple.Disc1 = "false";
                        break;
                    case "SV003":
                        couple.LoveLanguage = "false";
                        couple.LoveLanguage1 = "false";
                        break;
                    case "SV004":
                        couple.BigFive = "false";
                        couple.BigFive1 = "false";
                        break;
                }
            }
        }

        await _coupleRepository.CreateAsync(couple);

        return ServiceResponse<string>.SuccessResponse(couple.Id);
    }

    public async Task<ServiceResponse<string>> CancelLatestCoupleAsync(string memberId)
    {
        var couple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(memberId);

        if (couple == null)
            return ServiceResponse<string>.ErrorResponse("Bạn không có phòng nào để hủy.");

        if (couple.Status != 1)
            return ServiceResponse<string>.ErrorResponse("Không có phòng sao mà hủy.");

        couple.Status = 0;
        await _coupleRepository.UpdateAsync(couple);

        return ServiceResponse<string>.SuccessResponse("Đã hủy phòng thành công.");
    }

    public async Task<ServiceResponse<CoupleDetailResponse>> GetLatestCoupleDetailAsync(string memberId)
    {
        var couple = await _coupleRepository.GetLatestCoupleByMemberIdWithMembersAsync(memberId);

        if (couple == null)
            return ServiceResponse<CoupleDetailResponse>.ErrorResponse("Bạn chưa có phòng nào.");

        var dto = _mapper.Map<CoupleDetailResponse>(couple);

        dto.IsOwned = couple.Member == memberId;

        return ServiceResponse<CoupleDetailResponse>.SuccessResponse(dto);
    }

    public async Task<ServiceResponse<int?>> GetLatestCoupleStatusAsync(string memberId)
    {
        var status = await _coupleRepository.GetLatestCoupleStatusByMemberIdAsync(memberId);

        if (status == null)
            return ServiceResponse<int?>.ErrorResponse("Bạn chưa có phòng nào.");

        return ServiceResponse<int?>.SuccessResponse(status);
    }


    public async Task<ServiceResponse<string>> SubmitResultAsync(string memberId, SurveyResultRequest request)
    {
        var couple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(memberId);
        if (couple == null)
            return ServiceResponse<string>.ErrorResponse("Couple not found.");

        var personTypes = await _personTypeRepo.GetPersonTypesBySurveyAsync(request.SurveyId);
        var personTypeDict = personTypes.ToDictionary(x => x.Name, x => x);
        string resultType = null;
        string description = "";
        string detail = string.Join(",", request.Answers.Select(a => $"{a.Tag}:{a.Score}"));

        // ✅ Tính resultType theo Survey
        if (request.SurveyId == "SV001") // MBTI
        {
            var mbtiLetters = new List<string>
        {
            request.Answers.Where(x => x.Tag == "E").Sum(x => x.Score) >= request.Answers.Where(x => x.Tag == "I").Sum(x => x.Score) ? "E" : "I",
            request.Answers.Where(x => x.Tag == "N").Sum(x => x.Score) >= request.Answers.Where(x => x.Tag == "S").Sum(x => x.Score) ? "N" : "S",
            request.Answers.Where(x => x.Tag == "T").Sum(x => x.Score) >= request.Answers.Where(x => x.Tag == "F").Sum(x => x.Score) ? "T" : "F",
            request.Answers.Where(x => x.Tag == "J").Sum(x => x.Score) >= request.Answers.Where(x => x.Tag == "P").Sum(x => x.Score) ? "J" : "P",
        };
            resultType = string.Join("", mbtiLetters);
        }
        else
        {
            var highest = request.Answers.OrderByDescending(x => x.Score).FirstOrDefault();
            resultType = highest?.Tag;
        }

        if (string.IsNullOrEmpty(resultType) || !personTypeDict.ContainsKey(resultType))
            return ServiceResponse<string>.ErrorResponse("Unable to determine result.");

        description = personTypeDict[resultType].Description ?? "No description available.";
        // ✅ Ghi kết quả vào Couple
        if (couple.Member == memberId)
        {
            switch (request.SurveyId)
            {
                case "SV001": couple.Mbti = resultType; couple.MbtiDescription = detail; break;
                case "SV002": couple.Disc = resultType; couple.DiscDescription = detail; break;
                case "SV003": couple.LoveLanguage = resultType; couple.LoveLanguageDescription = detail; break;
                case "SV004": couple.BigFive = resultType; couple.BigFiveDescription = detail; break;
            }
        }
        else if (couple.Member1 == memberId)
        {
            switch (request.SurveyId)
            {
                case "SV001": couple.Mbti1 = resultType; couple.Mbti1Description = detail; break;
                case "SV002": couple.Disc1 = resultType; couple.Disc1Description = detail; break;
                case "SV003": couple.LoveLanguage1 = resultType; couple.LoveLanguage1Description = detail; break;
                case "SV004": couple.BigFive1 = resultType; couple.BigFive1Description = detail; break;
            }
        }

        // ✅ Check xem tất cả các survey cần làm đã hoàn thành chưa
        var surveyMap = new List<(string SurveyId, string Type1, string Type2, Action<string> SetResult)>
    {
        ("SV001", couple.Mbti, couple.Mbti1, (id) => couple.MbtiResult = id),
        ("SV002", couple.Disc, couple.Disc1, (id) => couple.DiscResult = id),
        ("SV003", couple.LoveLanguage, couple.LoveLanguage1, (id) => couple.LoveLanguageResult = id),
        ("SV004", couple.BigFive, couple.BigFive1, (id) => couple.BigFiveResult = id),
    };

        bool allCompleted = true;

        foreach (var survey in surveyMap)
        {
            if (survey.Type1 == null && survey.Type2 == null) continue;

            if (survey.Type1 == null || survey.Type2 == null ||
                survey.Type1 == "false" || survey.Type2 == "false")
            {
                allCompleted = false;
                break;
            }
        }

        // ✅ Nếu đã xong hết, tính Result cho tất cả
        if (allCompleted)
        {
            foreach (var survey in surveyMap)
            {
                if (survey.Type1 == null || survey.Type2 == null ||
                    survey.Type1 == "false" || survey.Type2 == "false") continue;

                var result = await _resultPersonTypeRepo.FindResultAsync(survey.SurveyId, survey.Type1, survey.Type2);
                if (result != null)
                {
                    survey.SetResult(result.Id);
                }
            }
        }

        await _coupleRepository.UpdateAsync(couple);

        return ServiceResponse<string>.SuccessResponse($"Bạn thuộc kiểu {resultType} : {description}");
    }

    public async Task<ServiceResponse<PartnerSurveySimpleProgressDto>> CheckPartnerAllSurveysStatusAsync(string memberId)
    {
        var couple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(memberId);
        if (couple == null)
        {
            return ServiceResponse<PartnerSurveySimpleProgressDto>.ErrorResponse("Couple not found.");
        }

        bool isMember = couple.Member == memberId;
        string partnerId = isMember ? couple.Member1 : couple.Member;

        // ✅ Partner survey values
        var partnerSurveys = new List<string>
    {
        isMember ? couple.Mbti1 : couple.Mbti,
        isMember ? couple.Disc1 : couple.Disc,
        isMember ? couple.LoveLanguage1 : couple.LoveLanguage,
        isMember ? couple.BigFive1 : couple.BigFive
    };

        int partnerTotal = 0, partnerDone = 0;
        foreach (var val in partnerSurveys)
        {
            if (val == null) continue;
            partnerTotal++;
            if (val != "false") partnerDone++;
        }

        // ✅ Self survey values
        var selfSurveys = new List<string>
    {
        isMember ? couple.Mbti : couple.Mbti1,
        isMember ? couple.Disc : couple.Disc1,
        isMember ? couple.LoveLanguage : couple.LoveLanguage1,
        isMember ? couple.BigFive : couple.BigFive1
    };

        int selfTotal = 0, selfDone = 0;
        foreach (var val in selfSurveys)
        {
            if (val == null) continue;
            selfTotal++;
            if (val != "false") selfDone++;
        }

        var dto = new PartnerSurveySimpleProgressDto
        {
            PartnerTotalDone = partnerDone,
            PartnerTotalSurveys = partnerTotal,
            IsPartnerDoneAll = partnerTotal > 0 && partnerDone == partnerTotal,

            SelfTotalDone = selfDone,
            SelfTotalSurveys = selfTotal,
            IsSelfDoneAll = selfTotal > 0 && selfDone == selfTotal
        };

        return ServiceResponse<PartnerSurveySimpleProgressDto>.SuccessResponse(dto);
    }

}