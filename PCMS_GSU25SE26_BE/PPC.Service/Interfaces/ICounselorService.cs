using PPC.Service.ModelRequest.WorkScheduleRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using PPC.Service.ModelResponse.WorkScheduleResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ICounselorService
    {
        Task<ServiceResponse<List<CounselorDto>>> GetAllCounselorsAsync();
        Task CheckAndUpdateCounselorStatusAsync(string counselorId);
        Task<ServiceResponse<List<CounselorWithSubDto>>> GetActiveCounselorsWithSubAsync();
        Task<ServiceResponse<AvailableScheduleOverviewDto>> GetAvailableScheduleAsync(GetAvailableScheduleRequest request);

    }
}
