using PPC.Service.ModelRequest.Couple;
using PPC.Service.ModelRequest.SurveyRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.Couple;
using PPC.Service.ModelResponse.CoupleResponse;
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
        Task<ServiceResponse<string>> SubmitResultAsync(string memberId, SurveyResultRequest request);
        Task<ServiceResponse<PartnerSurveySimpleProgressDto>> CheckPartnerAllSurveysStatusAsync(string memberId);
        Task<ServiceResponse<List<CoupleDetailResponse>>> GetCouplesByMemberIdAsync(string memberId);
        Task<ServiceResponse<CoupleResultDto>> GetCoupleResultByIdAsync(string coupleId, string currentMemberId);
        Task<ServiceResponse<string>> MarkCoupleAsCompletedAsync(string coupleId);
        Task<ServiceResponse<string>> CreateVirtualCoupleAsync(string memberId, VirtualCoupleCreateRequest request);
        Task<ServiceResponse<string>> SubmitVirtualResultAsync(string memberId, SurveyResultRequest request);
        Task<ServiceResponse<string>> ApplyLatestResultToSelfAsync(string memberId, string surveyId);
        Task<ServiceResponse<CoupleResultDto>> GetCoupleResultByBookingIdAsync(string bookingId);
        Task<ServiceResponse<List<string>>> GetSubCategoryNamesByCoupleIdAsync(string coupleId);
    }
}
