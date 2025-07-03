using PPC.Service.ModelRequest.Couple;
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
        Task<ServiceResponse<string>> JoinCoupleByAccessCodeAsync(string memberId, string accessCode);
        Task<ServiceResponse<CoupleDetailResponse>> GetCoupleDetailAsync(string coupleId);
        Task<ServiceResponse<string>> CreateCoupleAsync(string memberId, CoupleCreateRequest request);
        Task<ServiceResponse<string>> CancelLatestCoupleAsync(string memberId);
        Task<ServiceResponse<CoupleDetailResponse>> GetLatestCoupleDetailAsync(string memberId);
        Task<ServiceResponse<int?>> GetLatestCoupleStatusAsync(string memberId);

    }
}
