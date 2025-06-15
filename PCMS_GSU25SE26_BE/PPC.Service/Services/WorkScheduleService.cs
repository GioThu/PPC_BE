using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly IWorkScheduleRepository _workScheduleRepository;
        private readonly IMapper _mapper;


        public WorkScheduleService(IWorkScheduleRepository workScheduleRepository, IMapper mapper)
        {
            _workScheduleRepository = workScheduleRepository;
            _mapper = mapper;

        }

        public async Task<ServiceResponse<string>> CreateScheduleAsync(string counselorId, WorkScheduleCreateRequest request)
        {
            var workDate = request.WorkDate.Date;
            var startTime = request.StartTime;
            var endTime = request.EndTime;

            var schedules = await _workScheduleRepository.GetSchedulesByCounselorIdAsync(counselorId);
            var sameDaySchedules = schedules
                .Where(ws => ws.WorkDate == workDate)
                .ToList();

            bool isOverlapping = sameDaySchedules.Any(ws =>
                (startTime < ws.EndTime && endTime > ws.StartTime) &&
                !(startTime == ws.EndTime || endTime == ws.StartTime)
            );

            if (isOverlapping)
                return ServiceResponse<string>.ErrorResponse("Work schedule overlaps with an existing one.");

            var toMerge = sameDaySchedules
                .Where(ws => ws.EndTime == startTime || ws.StartTime == endTime)
                .OrderBy(ws => ws.StartTime)
                .ToList();

            if (toMerge.Any())
            {
                var newStart = toMerge.Select(ws => ws.StartTime).Append(startTime).Min();
                var newEnd = toMerge.Select(ws => ws.EndTime).Append(endTime).Max();

                var mainSchedule = toMerge.First();
                mainSchedule.StartTime = newStart;
                mainSchedule.EndTime = newEnd;
                mainSchedule.Description = string.IsNullOrEmpty(request.Description) ? mainSchedule.Description : request.Description;
                mainSchedule.CreateAt = Utils.Utils.GetTimeNow(); 

                var toDelete = toMerge.Skip(1).ToList();
                foreach (var s in toDelete)
                {
                    await _workScheduleRepository.DeleteScheduleByIdAsync(s.Id);
                }

                await _workScheduleRepository.UpdateAsync(mainSchedule);

                return ServiceResponse<string>.SuccessResponse("Schedule merged successfully.");
            }
            else
            {
                var schedule = request.ToCreateWorkSchedule();
                schedule.CounselorId = counselorId;

                await _workScheduleRepository.CreateAsync(schedule);
                return ServiceResponse<string>.SuccessResponse("Schedule created successfully.");
            }
        }

        public async Task<ServiceResponse<List<WorkScheduleDto>>> GetSchedulesByCounselorAsync(string counselorId)
        {
            var schedules = await _workScheduleRepository.GetSchedulesByCounselorIdAsync(counselorId);
            var scheduleDtos = _mapper.Map<List<WorkScheduleDto>>(schedules);
            return ServiceResponse<List<WorkScheduleDto>>.SuccessResponse(scheduleDtos);
        }

        public async Task<ServiceResponse<string>> DeleteScheduleAsync(string counselorId, string scheduleId)
        {
            var schedules = await _workScheduleRepository.GetSchedulesByCounselorIdAsync(counselorId);
            var schedule = schedules.FirstOrDefault(s => s.Id == scheduleId);

            if (schedule == null)
                return ServiceResponse<string>.ErrorResponse("Schedule not found or access denied.");

            var success = await _workScheduleRepository.DeleteScheduleByIdAsync(scheduleId);
            if (!success)
                return ServiceResponse<string>.ErrorResponse("Failed to delete schedule.");

            return ServiceResponse<string>.SuccessResponse("Schedule deleted successfully.");
        }
    }
}