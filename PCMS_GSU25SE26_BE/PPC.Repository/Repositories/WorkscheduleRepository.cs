using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using PPC.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Repositories
{
    public class WorkScheduleRepository : GenericRepository<WorkSchedule>, IWorkScheduleRepository
    {
        public WorkScheduleRepository(CCPContext context) : base(context)
        {
        }

        public async Task<bool> IsScheduleOverlappingAsync(string counselorId, DateTime workDate, DateTime startTime, DateTime endTime)
        {
            return await _context.WorkSchedules
                .AnyAsync(ws =>
                    ws.CounselorId == counselorId &&
                    ws.WorkDate == workDate &&
                    (
                        (startTime >= ws.StartTime && startTime < ws.EndTime) ||
                        (endTime > ws.StartTime && endTime <= ws.EndTime) ||
                        (startTime <= ws.StartTime && endTime >= ws.EndTime)
                    )
                );
        }

        public async Task<List<WorkSchedule>> GetSchedulesByCounselorIdAsync(string counselorId)
        {
            return await _context.WorkSchedules
                .Where(ws => ws.CounselorId == counselorId)
                .OrderBy(ws => ws.WorkDate).ThenBy(ws => ws.StartTime)
                .ToListAsync();
        }
        public async Task<bool> DeleteScheduleByIdAsync(string scheduleId)
        {
            var schedule = await _context.WorkSchedules.FindAsync(scheduleId);
            if (schedule == null) return false;

            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<WorkSchedule>> GetByCounselorAndDateAsync(string counselorId, DateTime date)
        {
            return await _context.WorkSchedules
                .Where(ws => ws.CounselorId == counselorId && ws.WorkDate.HasValue && ws.WorkDate.Value.Date == date.Date)
                .ToListAsync();
        }

        public async Task<List<WorkSchedule>> GetByCounselorBetweenDatesAsync(string counselorId, DateTime from, DateTime to)
        {
            return await _context.WorkSchedules
                .Where(ws => ws.CounselorId == counselorId && ws.WorkDate >= from && ws.WorkDate <= to)
                .ToListAsync();
        }
    }
}
