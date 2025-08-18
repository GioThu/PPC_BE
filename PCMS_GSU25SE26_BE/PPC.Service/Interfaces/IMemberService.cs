using PPC.Service.ModelRequest;
using PPC.Service.ModelRequest.AccountRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.MemberResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface  IMemberService
    {
        Task<ServiceResponse<PagingResponse<MemberDto>>> GetAllPagingAsync(PagingRequest request);
        Task<ServiceResponse<string>> UpdateStatusAsync(MemberStatusUpdateRequest request);
        Task<ServiceResponse<MemberProfileDto>> GetMyProfileAsync(string accountId);
        Task<ServiceResponse<string>> UpdateMyProfileAsync(string accountId, MemberProfileUpdateRequest request);
        Task<ServiceResponse<List<string>>> GetRecommendedSubCategoriesAsync(string memberId);
    }
}
