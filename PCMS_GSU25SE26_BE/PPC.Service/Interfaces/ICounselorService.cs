using PPC.Service.ModelRequest;
using PPC.Service.ModelResponse;
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
        Task<ServiceResponse<AvailableScheduleDto>> GetAvailableScheduleAsync(GetAvailableScheduleRequest request);

    }
}
