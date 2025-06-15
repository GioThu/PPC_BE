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
            if (await _workScheduleRepository.IsScheduleOverlappingAsync(counselorId, request.WorkDate.Date, request.StartTime, request.EndTime))
            {
                return ServiceResponse<string>.ErrorResponse("Work schedule overlaps with an existing one.");
            }

            var schedule = request.ToCreateWorkSchedule();
            schedule.CounselorId = counselorId;

            await _workScheduleRepository.CreateAsync(schedule);
            return ServiceResponse<string>.SuccessResponse("Work schedule created successfully.");
        }

        public async Task<ServiceResponse<List<WorkScheduleDto>>> GetSchedulesByCounselorAsync(string counselorId)
        {
            var schedules = await _workScheduleRepository.GetSchedulesByCounselorIdAsync(counselorId);
            var scheduleDtos = _mapper.Map<List<WorkScheduleDto>>(schedules);
            return ServiceResponse<List<WorkScheduleDto>>.SuccessResponse(scheduleDtos);
        }
    }
}