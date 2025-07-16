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

    public CoupleService(ICoupleRepository coupleRepository, IMapper mapper, IMemberRepository memberRepo, IPersonTypeRepository personTypeRepo)
    {
        _coupleRepository = coupleRepository;
        _mapper = mapper;
        _memberRepo = memberRepo;
        _personTypeRepo = personTypeRepo;
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
        var personTypes = await _personTypeRepo.GetPersonTypesBySurveyAsync(request.SurveyId);

        var description = "";
        string resultType = null;

        var couple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(memberId);
        if (couple == null)
        {
            return ServiceResponse<string>.ErrorResponse("Couple not found.");
        }

        var detail = string.Join(",", request.Answers
            .Select(a => $"{a.Tag}:{a.Score}"));

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

            resultType = matched.Name;
            description = !string.IsNullOrEmpty(matched.Description)
                ? matched.Description
                : "No description available.";

            if (couple.Member == memberId)
            {
                couple.Mbti = matched.Name;
                couple.MbtiDescription = detail;
            }
            else if (couple.Member1 == memberId)
            {
                couple.Mbti1 = matched.Name;
                couple.Mbti1Description = detail;
            }
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
                    resultType = matched.Name;
                    description = !string.IsNullOrEmpty(matched.Description)
                        ? matched.Description
                        : "No description available.";

                    if (couple.Member == memberId)
                    {
                        switch (request.SurveyId)
                        {
                            case "SV002":
                                couple.Disc = matched.Name;
                                couple.DiscDescription = detail;
                                break;
                            case "SV003":
                                couple.LoveLanguage = matched.Name;
                                couple.LoveLanguageDescription = detail;
                                break;
                            case "SV004":
                                couple.BigFive = matched.Name;
                                couple.BigFiveDescription = detail;
                                break;
                        }
                    }
                    else if (couple.Member1 == memberId)
                    {
                        switch (request.SurveyId)
                        {
                            case "SV002":
                                couple.Disc1 = matched.Name;
                                couple.Disc1Description = detail;
                                break;
                            case "SV003":
                                couple.LoveLanguage1 = matched.Name;
                                couple.LoveLanguage1Description = detail;
                                break;
                            case "SV004":
                                couple.BigFive1 = matched.Name;
                                couple.BigFive1Description = detail;
                                break;
                        }
                    }
                }
            }
        }

        if (resultType == null)
        {
            return ServiceResponse<string>.ErrorResponse("Unable to determine result.");
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

        var surveys = new Dictionary<string, string>
    {
        { "SV001", isMember ? couple.Mbti1 : couple.Mbti },
        { "SV002", isMember ? couple.Disc1 : couple.Disc },
        { "SV003", isMember ? couple.LoveLanguage1 : couple.LoveLanguage },
        { "SV004", isMember ? couple.BigFive1 : couple.BigFive }
    };

        int totalSurveys = 0;
        int totalDone = 0;

        foreach (var survey in surveys)
        {
            var value = survey.Value;

            if (value == null)
            {
                continue;
            }

            // Có survey này
            totalSurveys++;

            if (value != "false")
            {
                totalDone++;
            }
        }

        var dto = new PartnerSurveySimpleProgressDto
        {
            TotalDone = totalDone,
            TotalSurveys = totalSurveys,
            IsDoneAll = totalSurveys > 0 && totalDone == totalSurveys
        };

        return ServiceResponse<PartnerSurveySimpleProgressDto>.SuccessResponse(dto);
    }

}