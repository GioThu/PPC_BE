using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.Couple;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.Couple;
using PPC.Service.ModelResponse.MemberResponse;
using PPC.Service.Utils;

public class CoupleService : ICoupleService
{
    private readonly ICoupleRepository _coupleRepository;
    private readonly IMapper _mapper;

    public CoupleService(ICoupleRepository coupleRepository, IMapper mapper)
    {
        _coupleRepository = coupleRepository;
        _mapper = mapper;
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
}