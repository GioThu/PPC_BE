using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
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
    private readonly IResultHistoryRepository _resultHistoryRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly ISubCategoryRepository _subCategoryRepository;



    public CoupleService(ICoupleRepository coupleRepository, IMapper mapper, IMemberRepository memberRepo, IPersonTypeRepository personTypeRepo, IResultPersonTypeRepository resultPersonTypeRepo, IResultHistoryRepository resultHistoryRepo, IBookingRepository bookingRepo, ISubCategoryRepository subCategoryRepository)
    {
        _coupleRepository = coupleRepository;
        _mapper = mapper;
        _memberRepo = memberRepo;
        _personTypeRepo = personTypeRepo;
        _resultPersonTypeRepo = resultPersonTypeRepo;
        _resultHistoryRepo = resultHistoryRepo;
        _bookingRepo = bookingRepo;
        _subCategoryRepository = subCategoryRepository;
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
            return ServiceResponse<string>.ErrorResponse("Không tìm thấy phòng");

        if (couple.IsVirtual == true)
            return ServiceResponse<string>.ErrorResponse("Đây là phòng cho người ảo. Không thể tham gia");

        if (couple.Member == memberId)
            return ServiceResponse<string>.ErrorResponse("Bạn không thể tham gia phòng của chính mình");

        if (!string.IsNullOrEmpty(couple.Member1))
            return ServiceResponse<string>.ErrorResponse("Phòng này đã có người tham gia cùng");

        var ownerLatestCouple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(couple.Member);
        if (ownerLatestCouple == null || ownerLatestCouple.Id != couple.Id || ownerLatestCouple.Status != 1)
        {
            return ServiceResponse<string>.ErrorResponse("This room is not the latest active room of the owner. Cannot join.");
        }

        couple.Member1 = memberId;
        await _coupleRepository.UpdateAsync(couple);

        return ServiceResponse<string>.SuccessResponse("Tham gia phòng thành công");
    }

    public async Task<ServiceResponse<CoupleDetailResponse>> GetCoupleDetailAsync(string coupleId)
    {
        var couple = await _coupleRepository.GetCoupleByIdWithMembersAsync(coupleId);

        if (couple == null)
            return ServiceResponse<CoupleDetailResponse>.ErrorResponse("Không tìm thấy cặp đôi");

        var dto = _mapper.Map<CoupleDetailResponse>(couple);

        return ServiceResponse<CoupleDetailResponse>.SuccessResponse(dto);
    }

    public async Task<ServiceResponse<string>> CreateCoupleAsync(string memberId, CoupleCreateRequest request)
    {
        var hasActive = await _coupleRepository.HasActiveCoupleAsync(memberId);
        if (hasActive)
        {
            return ServiceResponse<string>.ErrorResponse("Bạn đã có một phòng đang hoạt động. Không thể tạo phòng mới");
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
            return ServiceResponse<string>.ErrorResponse("Không tìm thấy cặp đôi");

        var personTypes = await _personTypeRepo.GetPersonTypesBySurveyAsync(request.SurveyId);
        var personTypeDict = personTypes.ToDictionary(x => x.Name, x => x);
        string resultType = null;
        string description = "";
        if (request.Answers == null || !request.Answers.Any())
            return ServiceResponse<string>.ErrorResponse("Không có câu trả lời nào");

        var detail = string.Join(",", request.Answers
            .Where(a => !string.IsNullOrEmpty(a.Tag))
            .Select(a => $"{a.Tag}:{a.Score}"));


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
            return ServiceResponse<string>.ErrorResponse("Không thể xác định kết quả");

        description = personTypeDict[resultType].Description ?? "Không có mô tả";

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

        if (allCompleted)
        {
            // Gom các result đã tìm được của những cặp hoàn chỉnh
            var successfulResults = new List<ResultPersonType>();

            foreach (var survey in surveyMap)
            {
                if (survey.Type1 == null || survey.Type2 == null ||
                    survey.Type1 == "false" || survey.Type2 == "false")
                    continue;

                var result = await _resultPersonTypeRepo.FindResultAsync(survey.SurveyId, survey.Type1, survey.Type2);
                if (result != null)
                {
                    survey.SetResult(result.Id);
                    successfulResults.Add(result);
                }
            }

            var recCats = successfulResults
                .Where(r => !string.IsNullOrWhiteSpace(r.CategoryId))
                .GroupBy(r => r.CategoryId)
                .Select(g => g.OrderBy(x => x.Compatibility).First())
                .OrderBy(r => r.Compatibility)
                .ThenBy(r => r.Id)
                .Take(2)
                .Select(r => r.CategoryId)
                .ToList();

            if (recCats.Count > 0) couple.Rec1 = recCats[0];
            if (recCats.Count > 1) couple.Rec2 = recCats[1];
        }
        await _coupleRepository.UpdateAsync(couple);

        return ServiceResponse<string>.SuccessResponse($"Bạn thuộc kiểu {resultType} : {description}");
    }

    public async Task<ServiceResponse<CoupleResultDto>> GetCoupleResultByIdAsync(string coupleId, string currentMemberId)
    {
        var couple = await _coupleRepository.GetCoupleWithMembersByIdAsync(coupleId);
        if (couple == null)
            return ServiceResponse<CoupleResultDto>.ErrorResponse("Không tìm thấy cặp đôi");

        var result = new CoupleResultDto
        {
            Id = couple.Id,
            IsOwned = couple.Member == currentMemberId,
            Member = _mapper.Map<MemberDto>(couple.MemberNavigation),
            Member1 = _mapper.Map<MemberDto>(couple.Member1Navigation),
            Mbti = couple.Mbti,
            Disc = couple.Disc,
            LoveLanguage = couple.LoveLanguage,
            BigFive = couple.BigFive,
            Mbti1 = couple.Mbti1,
            Disc1 = couple.Disc1,
            LoveLanguage1 = couple.LoveLanguage1,
            BigFive1 = couple.BigFive1,
            MbtiDescription = couple.MbtiDescription,
            DiscDescription = couple.DiscDescription,
            LoveLanguageDescription = couple.LoveLanguageDescription,
            BigFiveDescription = couple.BigFiveDescription,
            Mbti1Description = couple.Mbti1Description,
            Disc1Description = couple.Disc1Description,
            LoveLanguage1Description = couple.LoveLanguage1Description,
            BigFive1Description = couple.BigFive1Description,
            MbtiResult = couple.MbtiResult,
            DiscResult = couple.DiscResult,
            LoveLanguageResult = couple.LoveLanguageResult,
            BigFiveResult = couple.BigFiveResult,
            IsVirtual = couple.IsVirtual,
            VirtualName = couple.VirtualName,
            VirtualDob = couple.VirtualDob,
            VirtualAvatar = couple.VirtualAvatar,
            VirtualDescription = couple.VirtualDescription,
            VirtualGender = couple.VirtualGender,
            VirtualRelationship = couple.VirtualRelationship,
            CreateAt = couple.CreateAt,
            Rec1 = couple.Rec1,
            Rec2 = couple.Rec2,
            Status = couple.Status,
            AccessCode = couple.AccessCode
        };

        async Task<ResultPersonTypeDto> LoadResult(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var entity = await _resultPersonTypeRepo.GetByIdWithIncludesAsync(id);
            return entity == null ? null : _mapper.Map<ResultPersonTypeDto>(entity);
        }

        result.MbtiDetail = await LoadResult(couple.MbtiResult);
        result.DiscDetail = await LoadResult(couple.DiscResult);
        result.LoveLanguageDetail = await LoadResult(couple.LoveLanguageResult);
        result.BigFiveDetail = await LoadResult(couple.BigFiveResult);

        return ServiceResponse<CoupleResultDto>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<CoupleResultDto>> GetCoupleResultByBookingIdAsync(string bookingId)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);
        if (booking == null)
            return ServiceResponse<CoupleResultDto>.ErrorResponse("Không tìm thấy lịch hẹn");

        if (string.IsNullOrWhiteSpace(booking.MemberId) || string.IsNullOrWhiteSpace(booking.Member2Id))
            return ServiceResponse<CoupleResultDto>.ErrorResponse("Lịch hẹn không hợp lệ cho cặp đôi");

        var memberA = booking.MemberId;
        var memberB = booking.Member2Id;

        // Lấy couple mới nhất của 2 người, status = 2
        var couple = await _coupleRepository.GetLatestCoupleByMembersWithIncludesAsync(memberA, memberB, status: 2);
        if (couple == null)
            return ServiceResponse<CoupleResultDto>.ErrorResponse("Không tìm thấy bất kì lịch sử survey cặp đôi phù hợp");

        var result = new CoupleResultDto
        {
            Id = couple.Id,
            // Không có currentMemberId, set mặc định (nếu thuộc tính bắt buộc)
            IsOwned = false,
            Member = _mapper.Map<MemberDto>(couple.MemberNavigation),
            Member1 = _mapper.Map<MemberDto>(couple.Member1Navigation),
            Mbti = couple.Mbti,
            Disc = couple.Disc,
            LoveLanguage = couple.LoveLanguage,
            BigFive = couple.BigFive,
            Mbti1 = couple.Mbti1,
            Disc1 = couple.Disc1,
            LoveLanguage1 = couple.LoveLanguage1,
            BigFive1 = couple.BigFive1,
            MbtiDescription = couple.MbtiDescription,
            DiscDescription = couple.DiscDescription,
            LoveLanguageDescription = couple.LoveLanguageDescription,
            BigFiveDescription = couple.BigFiveDescription,
            Mbti1Description = couple.Mbti1Description,
            Disc1Description = couple.Disc1Description,
            LoveLanguage1Description = couple.LoveLanguage1Description,
            BigFive1Description = couple.BigFive1Description,
            MbtiResult = couple.MbtiResult,
            DiscResult = couple.DiscResult,
            LoveLanguageResult = couple.LoveLanguageResult,
            BigFiveResult = couple.BigFiveResult,
            IsVirtual = couple.IsVirtual,
            VirtualName = couple.VirtualName,
            VirtualDob = couple.VirtualDob,
            VirtualAvatar = couple.VirtualAvatar,
            VirtualDescription = couple.VirtualDescription,
            VirtualGender = couple.VirtualGender,
            VirtualRelationship = couple.VirtualRelationship,
            CreateAt = couple.CreateAt,
            Rec1 = couple.Rec1,
            Rec2 = couple.Rec2,
            Status = couple.Status,
            AccessCode = couple.AccessCode
        };

        async Task<ResultPersonTypeDto> LoadResult(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var entity = await _resultPersonTypeRepo.GetByIdWithIncludesAsync(id);
            return entity == null ? null : _mapper.Map<ResultPersonTypeDto>(entity);
        }

        result.MbtiDetail = await LoadResult(couple.MbtiResult);
        result.DiscDetail = await LoadResult(couple.DiscResult);
        result.LoveLanguageDetail = await LoadResult(couple.LoveLanguageResult);
        result.BigFiveDetail = await LoadResult(couple.BigFiveResult);

        return ServiceResponse<CoupleResultDto>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<PartnerSurveySimpleProgressDto>> CheckPartnerAllSurveysStatusAsync(string memberId)
    {
        var couple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(memberId);
        if (couple == null)
        {
            return ServiceResponse<PartnerSurveySimpleProgressDto>.ErrorResponse("Không tìm thấy cặp đôi");
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

    public async Task<ServiceResponse<List<CoupleDetailResponse>>> GetCouplesByMemberIdAsync(string memberId)
    {
        var couples = await _coupleRepository.GetCouplesByMemberIdAsync(memberId);
        var result = _mapper.Map<List<CoupleDetailResponse>>(couples);
        return ServiceResponse<List<CoupleDetailResponse>>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<string>> MarkCoupleAsCompletedAsync(string coupleId)
    {
        var couple = await _coupleRepository.GetByIdAsync(coupleId);
        if (couple == null)
            return ServiceResponse<string>.ErrorResponse("Không tìm thấy cặp đôi");

        couple.Status = 2;
        await _coupleRepository.UpdateAsync(couple);

        return ServiceResponse<string>.SuccessResponse("Cặp đôi đã được đánh dấu là hoàn tất");
    }

    public async Task<ServiceResponse<string>> CreateVirtualCoupleAsync(string memberId, VirtualCoupleCreateRequest request)
    {
        var hasActive = await _coupleRepository.HasActiveCoupleAsync(memberId);
        if (hasActive)
        {
            return ServiceResponse<string>.ErrorResponse("Bạn đã có một phòng đang hoạt động, không thể tạo phòng mới");
        }

        var couple = new Couple
        {
            Id = Utils.GenerateIdModel("Couple"),
            Member = memberId,
            AccessCode = Utils.GenerateAccessCode(),
            CreateAt = Utils.GetTimeNow(),
            Status = 1,
            IsVirtual = true,
            VirtualName = request.VirtualName,
            VirtualDob = request.VirtualDob,
            VirtualGender = request.VirtualGender,
            VirtualAvatar = request.VirtualAvatar,
            VirtualDescription = request.VirtualDescription,
            VirtualRelationship = request.VirtualRelationship

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

    public async Task<ServiceResponse<string>> SubmitVirtualResultAsync(string memberId, SurveyResultRequest request)
    {
        var couple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(memberId);
        if (couple == null || couple.IsVirtual != true)
            return ServiceResponse<string>.ErrorResponse("Không tìm thấy cặp đôi ảo");

        var personTypes = await _personTypeRepo.GetPersonTypesBySurveyAsync(request.SurveyId);
        var personTypeDict = personTypes.ToDictionary(x => x.Name, x => x);
        string resultType = null;
        string description = "";

        if (request.Answers == null || !request.Answers.Any())
            return ServiceResponse<string>.ErrorResponse("Không có câu trả lời nào được cung cấp");

        var detail = string.Join(",", request.Answers
            .Where(a => !string.IsNullOrEmpty(a.Tag))
            .Select(a => $"{a.Tag}:{a.Score}"));

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
            return ServiceResponse<string>.ErrorResponse("Không thể xác định kết quả");

        description = personTypeDict[resultType].Description ?? "Không có mô tả.";

        // ✅ Ghi kết quả vào Virtual (tức là Member1)
        switch (request.SurveyId)
        {
            case "SV001": couple.Mbti1 = resultType; couple.Mbti1Description = detail; break;
            case "SV002": couple.Disc1 = resultType; couple.Disc1Description = detail; break;
            case "SV003": couple.LoveLanguage1 = resultType; couple.LoveLanguage1Description = detail; break;
            case "SV004": couple.BigFive1 = resultType; couple.BigFive1Description = detail; break;
        }

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

        if (allCompleted)
        {
            // Gom các result đã tìm được của những cặp hoàn chỉnh
            var successfulResults = new List<ResultPersonType>();

            foreach (var survey in surveyMap)
            {
                if (survey.Type1 == null || survey.Type2 == null ||
                    survey.Type1 == "false" || survey.Type2 == "false")
                    continue;

                var result = await _resultPersonTypeRepo.FindResultAsync(survey.SurveyId, survey.Type1, survey.Type2);
                if (result != null)
                {
                    survey.SetResult(result.Id);
                    successfulResults.Add(result);
                }
            }

            var recCats = successfulResults
                .Where(r => !string.IsNullOrWhiteSpace(r.CategoryId))
                .GroupBy(r => r.CategoryId)
                .Select(g => g.OrderBy(x => x.Compatibility).First())
                .OrderBy(r => r.Compatibility)
                .ThenBy(r => r.Id)
                .Take(2)
                .Select(r => r.CategoryId)
                .ToList();

            if (recCats.Count > 0) couple.Rec1 = recCats[0];
            if (recCats.Count > 1) couple.Rec2 = recCats[1];
        }

        await _coupleRepository.UpdateAsync(couple);
        return ServiceResponse<string>.SuccessResponse($"Virtual thuộc kiểu {resultType} : {description}");
    }

    public async Task<ServiceResponse<string>> ApplyLatestResultToSelfAsync(string memberId, string surveyId)
    {
        var couple = await _coupleRepository.GetLatestCoupleByMemberIdAsync(memberId);
        if (couple == null)
            return ServiceResponse<string>.ErrorResponse("Không tìm thấy cặp đôi");

        var history = await _resultHistoryRepo.GetLatestResultAsync(memberId, surveyId);
        if (history == null)
            return ServiceResponse<string>.ErrorResponse("Không tìm thấy kết quả gần đây cho khảo sát này");

        // Ghi kết quả vào cặp
        if (couple.Member == memberId)
        {
            switch (surveyId)
            {
                case "SV001": couple.Mbti = history.Result; couple.MbtiDescription = history.Detail; break;
                case "SV002": couple.Disc = history.Result; couple.DiscDescription = history.Detail; break;
                case "SV003": couple.LoveLanguage = history.Result; couple.LoveLanguageDescription = history.Detail; break;
                case "SV004": couple.BigFive = history.Result; couple.BigFiveDescription = history.Detail; break;
            }
        }
        else if (couple.Member1 == memberId || couple.IsVirtual == true)
        {
            switch (surveyId)
            {
                case "SV001": couple.Mbti1 = history.Result; couple.Mbti1Description = history.Detail; break;
                case "SV002": couple.Disc1 = history.Result; couple.Disc1Description = history.Detail; break;
                case "SV003": couple.LoveLanguage1 = history.Result; couple.LoveLanguage1Description = history.Detail; break;
                case "SV004": couple.BigFive1 = history.Result; couple.BigFive1Description = history.Detail; break;
            }
        }

        // Danh sách các bài kiểm tra
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
            // Nếu cả 2 bên đều null, tức là không cần làm bài này
            if (survey.Type1 == null && survey.Type2 == null)
                continue;

            // Nếu một trong hai chưa làm hoặc bị đánh dấu "false" thì chưa xong
            if (survey.Type1 == null || survey.Type2 == null ||
                survey.Type1 == "false" || survey.Type2 == "false")
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            // Gom các result đã tìm được của những cặp hoàn chỉnh
            var successfulResults = new List<ResultPersonType>();

            foreach (var survey in surveyMap)
            {
                if (survey.Type1 == null || survey.Type2 == null ||
                    survey.Type1 == "false" || survey.Type2 == "false")
                    continue;

                var result = await _resultPersonTypeRepo.FindResultAsync(survey.SurveyId, survey.Type1, survey.Type2);
                if (result != null)
                {
                    survey.SetResult(result.Id);
                    successfulResults.Add(result);
                }
            }

            var recCats = successfulResults
                .Where(r => !string.IsNullOrWhiteSpace(r.CategoryId))
                .GroupBy(r => r.CategoryId)                    
                .Select(g => g.OrderBy(x => x.Compatibility).First())
                .OrderBy(r => r.Compatibility)
                .ThenBy(r => r.Id)                               
                .Take(2)
                .Select(r => r.CategoryId)
                .ToList();

            if (recCats.Count > 0) couple.Rec1 = recCats[0];
            if (recCats.Count > 1) couple.Rec2 = recCats[1];
        }

        await _coupleRepository.UpdateAsync(couple);

        return ServiceResponse<string>.SuccessResponse($"Áp dụng kết quả '{history.Result}' cho cặp đôi thành công.");
    }

    public async Task<ServiceResponse<List<string>>> GetSubCategoryNamesByCoupleIdAsync(string coupleId)
    {
        if (string.IsNullOrWhiteSpace(coupleId))
            return ServiceResponse<List<string>>.ErrorResponse("CoupleId is required.");

        var couple = await _coupleRepository.GetByIdAsync(coupleId);
        if (couple == null || couple.Status != 1)
            return ServiceResponse<List<string>>.SuccessResponse(new List<string>());

        var categoryIds = new List<string>();
        if (!string.IsNullOrWhiteSpace(couple.Rec1)) categoryIds.Add(couple.Rec1);
        if (!string.IsNullOrWhiteSpace(couple.Rec2)) categoryIds.Add(couple.Rec2);

        if (!categoryIds.Any())
            return ServiceResponse<List<string>>.SuccessResponse(new List<string>());

        var subs = await _subCategoryRepository.GetSubCategoriesByCategoryIdsAsync(categoryIds);
        var names = subs.Select(x => x.Name).ToList();

        return ServiceResponse<List<string>>.SuccessResponse(names);
    }
}