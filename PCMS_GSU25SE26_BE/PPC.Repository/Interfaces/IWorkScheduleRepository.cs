using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IWorkScheduleRepository : IGenericRepository<WorkSchedule>
    {
        Task<bool> IsScheduleOverlappingAsync(string counselorId, DateTime workDate, DateTime startTime, DateTime endTime);
        Task<List<WorkSchedule>> GetSchedulesByCounselorIdAsync(string counselorId);
        Task<bool> DeleteScheduleByIdAsync(string scheduleId);
        Task<List<WorkSchedule>> GetByCounselorAndDateAsync(string counselorId, DateTime date);
        Task<List<WorkSchedule>> GetByCounselorBetweenDatesAsync(string counselorId, DateTime from, DateTime to);

    }
}
