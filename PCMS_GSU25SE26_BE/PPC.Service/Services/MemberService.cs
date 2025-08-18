using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
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
        private readonly ISubCategoryRepository _subCategoryRepository;
        private readonly IMapper _mapper;

        public MemberService(IMemberRepository memberRepository, IMapper mapper, ISubCategoryRepository subCategoryRepository)
        {
            _memberRepository = memberRepository;
            _mapper = mapper;
            _subCategoryRepository = subCategoryRepository;
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
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy người dùng");

            member.Status = request.Status;

            var result = await _memberRepository.UpdateAsync(member);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Cập nhật trạng thái thất bại");

            var action = request.Status == 0 ? "blocked" : "unblocked";
            return ServiceResponse<string>.SuccessResponse($"Member {action} successfully.");
        }

        public async Task<ServiceResponse<MemberProfileDto>> GetMyProfileAsync(string accountId)
        {
            var member = await _memberRepository.GetByAccountIdAsync(accountId);
            if (member == null)
                return ServiceResponse<MemberProfileDto>.ErrorResponse("Không tìm thấy người dùng");

            return ServiceResponse<MemberProfileDto>.SuccessResponse(member.ToMemberProfileDto());
        }

        public async Task<ServiceResponse<string>> UpdateMyProfileAsync(string accountId, MemberProfileUpdateRequest request)
        {
            var member = await _memberRepository.GetByAccountIdAsync(accountId);
            if (member == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy người dùng");

            if (request.Fullname != null)
                member.Fullname = request.Fullname;

            if (request.Avatar != null)
                member.Avatar = request.Avatar;

            if (request.Phone != null)
                member.Phone = request.Phone;

            if (request.Dob != null)
                member.Dob = request.Dob;

            if (request.Gender != null)
                member.Gender = request.Gender;

            await _memberRepository.UpdateAsync(member);

            return ServiceResponse<string>.SuccessResponse("Hồ sơ đã được cập nhật thành công");
        }

        public async Task<ServiceResponse<List<string>>> GetRecommendedSubCategoriesAsync(string memberId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
                return ServiceResponse<List<string>>.ErrorResponse("Member not found");

            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(member.Rec1)) categoryIds.Add(member.Rec1);
            if (!string.IsNullOrEmpty(member.Rec2)) categoryIds.Add(member.Rec2);

            if (!categoryIds.Any())
                return ServiceResponse<List<string>>.SuccessResponse(new List<string>());

            var subCategories = await _subCategoryRepository.GetSubCategoriesByCategoryIdsAsync(categoryIds);
            var subCategoryNames = subCategories.Select(sc => sc.Name).ToList();

            return ServiceResponse<List<string>>.SuccessResponse(subCategoryNames);
        }
    }
}
