using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
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

    public async Task<ServiceResponse<string>> CreateCoupleAsync(string memberId)
    {
        var couple = new Couple
        {
            Id = Utils.GenerateIdModel("Couple"),
            Member = memberId,
            AccessCode = Utils.GenerateAccessCode(),
            CreateAt = Utils.GetTimeNow(),
            Status = 1
        };

        await _coupleRepository.CreateAsync(couple);

        return ServiceResponse<string>.SuccessResponse(couple.Id);
    }

    public async Task<ServiceResponse<List<CoupleRoomResponse>>> GetMyRoomsAsync(string memberId)
    {
        var couples = await _coupleRepository.GetCouplesByMemberIdWithMembersAsync(memberId);

        if (!couples.Any())
            return ServiceResponse<List<CoupleRoomResponse>>.ErrorResponse("No rooms found.");

        var rooms = new List<CoupleRoomResponse>();

        foreach (var couple in couples)
        {
            var isOwner = couple.Member == memberId;

            var room = new CoupleRoomResponse
            {
                Id = couple.Id,
                IsOwner = isOwner,
                AccessCode = couple.AccessCode,
                CreateAt = couple.CreateAt,
                Status = couple.Status,
                IsVirtual = couple.IsVirtual,
                VirtualName = couple.VirtualName,
                Member = couple.MemberNavigation != null
                    ? _mapper.Map<MemberDto>(couple.MemberNavigation)
                    : null,
                Member1 = couple.Member1Navigation != null
                    ? _mapper.Map<MemberDto>(couple.Member1Navigation)
                    : null
            };

            rooms.Add(room);
        }

        return ServiceResponse<List<CoupleRoomResponse>>.SuccessResponse(rooms);
    }
    public async Task<ServiceResponse<string>> JoinCoupleByAccessCodeAsync(string memberId, string accessCode)
    {
        var couple = await _coupleRepository.GetByAccessCodeAsync(accessCode);

        if (couple == null)
            return ServiceResponse<string>.ErrorResponse("Room not found.");

        if (couple.IsVirtual == true)
            return ServiceResponse<string>.ErrorResponse("This is a virtual room. Cannot join.");

        if (couple.Member == memberId)
            return ServiceResponse<string>.ErrorResponse("You cannot join your own room.");

        if (!string.IsNullOrEmpty(couple.Member1))
            return ServiceResponse<string>.ErrorResponse("This room already has a partner.");

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
}