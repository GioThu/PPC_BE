using PPC.DAO.Models;
using PPC.Service.ModelRequest.WorkScheduleRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.WorkScheduleResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IWorkScheduleService
    {
        Task<ServiceResponse<string>> CreateScheduleAsync(string counselorId, WorkScheduleCreateRequest request);
        Task<ServiceResponse<List<WorkScheduleDto>>> GetSchedulesByCounselorAsync(string counselorId);
        Task<ServiceResponse<string>> DeleteScheduleAsync(string counselorId, string scheduleId);

    }
}
