using PPC.Service.ModelRequest.MemberShipRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.MemberShipResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IMemberShipService
    {
        Task<ServiceResponse<string>> CreateMemberShipAsync(MemberShipCreateRequest request);
        Task<ServiceResponse<List<MemberShipDto>>> GetAllMemberShipsAsync();
        Task<ServiceResponse<string>> UpdateMemberShipAsync(MemberShipUpdateRequest request);
        Task<ServiceResponse<string>> DeleteMemberShipAsync(string id);
        Task<ServiceResponse<MemberBuyMemberShipResponse>> BuyMemberShipAsync(string memberId, string accountId, MemberBuyMemberShipRequest request);
        Task<ServiceResponse<List<MyMemberShipStatusResponse>>> GetMemberShipStatusAsync(string memberId);
        Task<int> GetMaxBookingDiscountByMemberAsync(string memberId);
        Task<int> GetMaxCourseDiscountByMemberAsync(string memberId);
    }
}
