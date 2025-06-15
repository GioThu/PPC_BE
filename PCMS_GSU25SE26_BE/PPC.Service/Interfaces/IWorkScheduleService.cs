using PPC.DAO.Models;
using PPC.Service.ModelRequest;
using PPC.Service.ModelResponse;
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

    }
}
