using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest;
using PPC.Service.ModelRequest.AccountRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.MemberResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IMapper _mapper;

        public MemberService(IMemberRepository memberRepository, IMapper mapper)
        {
            _memberRepository = memberRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<PagingResponse<MemberDto>>> GetAllPagingAsync(PagingRequest request)
        {
            var (members, total) = await _memberRepository.GetAllPagingAsync(request.PageNumber, request.PageSize, request.Status);
            var dtos = _mapper.Map<List<MemberDto>>(members);
            var paging = new PagingResponse<MemberDto>(dtos, total, request.PageNumber, request.PageSize);

            return ServiceResponse<PagingResponse<MemberDto>>.SuccessResponse(paging);
        }

        public async Task<ServiceResponse<string>> UpdateStatusAsync(MemberStatusUpdateRequest request)
        {
            var member = await _memberRepository.GetByIdAsync(request.MemberId);
            if (member == null)
                return ServiceResponse<string>.ErrorResponse("Member not found.");

            member.Status = request.Status;

            var result = await _memberRepository.UpdateAsync(member);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Failed to update status.");

            var action = request.Status == 0 ? "blocked" : "unblocked";
            return ServiceResponse<string>.SuccessResponse($"Member {action} successfully.");
        }

        public async Task<ServiceResponse<MemberProfileDto>> GetMyProfileAsync(string accountId)
        {
            var member = await _memberRepository.GetByAccountIdAsync(accountId);
            if (member == null)
                return ServiceResponse<MemberProfileDto>.ErrorResponse("Member not found.");

            return ServiceResponse<MemberProfileDto>.SuccessResponse(member.ToMemberProfileDto());
        }

        public async Task<ServiceResponse<string>> UpdateMyProfileAsync(string accountId, MemberProfileUpdateRequest request)
        {
            var member = await _memberRepository.GetByAccountIdAsync(accountId);
            if (member == null)
                return ServiceResponse<string>.ErrorResponse("Member not found.");

            // ❌ Không dùng mapper – update thủ công
            member.Fullname = request.Fullname;
            member.Avatar = request.Avatar;
            member.Phone = request.Phone;
            member.Dob = request.Dob;
            member.Gender = request.Gender;

            await _memberRepository.UpdateAsync(member);

            return ServiceResponse<string>.SuccessResponse("Profile updated successfully.");
        }
    }
}
