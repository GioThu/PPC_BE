using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.Couple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ICoupleService
    {
        Task<ServiceResponse<string>> CreateCoupleAsync(string memberId);
        Task<ServiceResponse<List<CoupleRoomResponse>>> GetMyRoomsAsync(string memberId);
        Task<ServiceResponse<string>> JoinCoupleByAccessCodeAsync(string memberId, string accessCode);
        Task<ServiceResponse<CoupleDetailResponse>> GetCoupleDetailAsync(string coupleId);
    }
}
