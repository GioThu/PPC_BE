using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest;
using PPC.Service.ModelRequest.AccountRequest;
using PPC.Service.ModelRequest.WorkScheduleRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using PPC.Service.ModelResponse.WorkScheduleResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class CounselorService : ICounselorService
    {
        private readonly ICounselorRepository _counselorRepository;
        private readonly IMapper _mapper;
        private readonly ICounselorSubCategoryRepository _counselorSubCategoryRepository;
        private readonly IWorkScheduleRepository _workScheduleRepo;
        private readonly IBookingRepository _bookingRepo;

        public CounselorService(ICounselorRepository counselorRepository, IMapper mapper, ICounselorSubCategoryRepository counselorSubCategoryRepository, IWorkScheduleRepository workScheduleRepo, IBookingRepository bookingRepo)
        {
            _counselorRepository = counselorRepository;
            _mapper = mapper;
            _counselorSubCategoryRepository = counselorSubCategoryRepository;
            _workScheduleRepo = workScheduleRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<ServiceResponse<List<CounselorDto>>> GetAllCounselorsAsync()
        {
            var counselors = await _counselorRepository.GetAllAsync();
            var counselorDtos = _mapper.Map<List<CounselorDto>>(counselors);
            return ServiceResponse<List<CounselorDto>>.SuccessResponse(counselorDtos);
        }

        public async Task CheckAndUpdateCounselorStatusAsync(string counselorId)
        {
            var counselor = await _counselorRepository.GetByIdAsync(counselorId);
            if (counselor == null) return;

            var hasApproved = await _counselorSubCategoryRepository
                .HasAnyApprovedSubCategoryAsync(counselorId);

            var newStatus = hasApproved ? 1 : 0;

            if (counselor.Status != newStatus)
            {
                counselor.Status = newStatus;
                await _counselorRepository.UpdateAsync(counselor);
            }
        }

        public async Task<ServiceResponse<List<CounselorWithSubDto>>> GetActiveCounselorsWithSubAsync()
        {
            var counselors = await _counselorRepository.GetActiveWithApprovedSubCategoriesAsync();

            var result = counselors.Select(c =>
            {
                var dto = _mapper.Map<CounselorWithSubDto>(c);

                var subCategories = c.CounselorSubCategories
                    .Where(csc => csc.Status == 1 && csc.SubCategory != null)
                    .GroupBy(csc => csc.SubCategory.Id) 
                    .Select(g => _mapper.Map<SubCategoryDto>(g.First().SubCategory))
                    .ToList();

                dto.SubCategories = subCategories;
                return dto;
            }).ToList();

            return ServiceResponse<List<CounselorWithSubDto>>.SuccessResponse(result);
        }

        public async Task<ServiceResponse<AvailableScheduleOverviewDto>> GetAvailableScheduleAsync(GetAvailableScheduleRequest request)
        {
            var counselor = await _counselorRepository.GetByIdAsync(request.CounselorId);
            if (counselor == null)
            {
                return ServiceResponse<AvailableScheduleOverviewDto>.ErrorResponse("Counselor not found.");
            }

            var counselorDto = _mapper.Map<CounselorDto>(counselor);

            var subCategories = await _counselorSubCategoryRepository
                .GetApprovedSubCategoriesByCounselorAsync(request.CounselorId);

            var subCategoryDtos = subCategories
                .GroupBy(sc => sc.Id)
                .Select(g => _mapper.Map<SubCategoryDto>(g.First()))
                .ToList();

            var startDate = Utils.Utils.GetTimeNow().Date;
            var endDate = startDate.AddDays(6);

            var workSchedules = await _workScheduleRepo.GetByCounselorBetweenDatesAsync(request.CounselorId, startDate, endDate);
            var bookings = await _bookingRepo.GetConfirmedBookingsBetweenDatesAsync(request.CounselorId, startDate, endDate);

            var workScheduleMap = workSchedules
                .Where(ws => ws.WorkDate.HasValue)
                .GroupBy(ws => ws.WorkDate.Value.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            var bookingMap = bookings
                .Where(b => b.TimeStart.HasValue)
                .GroupBy(b => b.TimeStart.Value.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dailySchedules = new List<DailyAvailableSlotDto>();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);

                var daySchedules = workScheduleMap.TryGetValue(currentDate, out var wsList)
                    ? wsList
                    : new List<WorkSchedule>();

                var dayBookings = bookingMap.TryGetValue(currentDate, out var bkList)
                    ? bkList
                    : new List<Booking>();

                var bookingIntervals = dayBookings
                    .Select(b => new
                    {
                        Start = b.TimeStart!.Value.TimeOfDay,
                        End = b.TimeEnd!.Value.TimeOfDay + TimeSpan.FromMinutes(10) // thêm buffer
                    })
                    .OrderBy(b => b.Start)
                    .ToList();

                var availableSlots = new List<AvailableTimeSlotDto>();

                foreach (var schedule in daySchedules)
                {
                    if (!schedule.StartTime.HasValue || !schedule.EndTime.HasValue)
                        continue;

                    var currentStart = schedule.StartTime.Value.TimeOfDay;
                    var scheduleEnd = schedule.EndTime.Value.TimeOfDay;

                    foreach (var b in bookingIntervals)
                    {
                        if (b.End <= currentStart) continue;
                        if (b.Start >= scheduleEnd) break;

                        if (b.Start > currentStart)
                        {
                            availableSlots.Add(new AvailableTimeSlotDto
                            {
                                Start = currentStart,
                                End = b.Start < scheduleEnd ? b.Start : scheduleEnd
                            });
                        }

                        currentStart = b.End > currentStart ? b.End : currentStart;
                    }

                    if (currentStart < scheduleEnd)
                    {
                        availableSlots.Add(new AvailableTimeSlotDto
                        {
                            Start = currentStart,
                            End = scheduleEnd
                        });
                    }
                }

                if (availableSlots.Any())
                {
                    if (currentDate == startDate)
                    {
                        var now = Utils.Utils.GetTimeNow().TimeOfDay;
                        availableSlots = availableSlots
                            .Where(slot => slot.End > now)
                            .ToList();
                    }

                    if (availableSlots.Any())
                    {
                        dailySchedules.Add(new DailyAvailableSlotDto
                        {
                            WorkDate = currentDate,
                            AvailableSlots = availableSlots.OrderBy(s => s.Start).ToList()
                        });
                    }
                }
            }

            var overviewDto = new AvailableScheduleOverviewDto
            {
                CounselorId = request.CounselorId,
                Counselor = counselorDto,
                SubCategories = subCategoryDtos,
                DailyAvailableSchedules = dailySchedules
            };

            return ServiceResponse<AvailableScheduleOverviewDto>.SuccessResponse(overviewDto);
        }

        public async Task<ServiceResponse<PagingResponse<CounselorDto>>> GetAllPagingAsync(PagingRequest request)
        {
            var (entities, totalCount) = await _counselorRepository.GetAllPagingAsync(
                request.PageNumber, request.PageSize, request.Status);

            var dtos = _mapper.Map<List<CounselorDto>>(entities);

            var paging = new PagingResponse<CounselorDto>(dtos, totalCount, request.PageNumber, request.PageSize);
            return ServiceResponse<PagingResponse<CounselorDto>>.SuccessResponse(paging);
        }

        public async Task<ServiceResponse<string>> UpdateStatusAsync(CounselorStatusUpdateRequest request)
        {
            var counselor = await _counselorRepository.GetByIdAsync(request.CounselorId);
            if (counselor == null)
                return ServiceResponse<string>.ErrorResponse("Counselor not found.");

            counselor.Status = request.Status;

            var result = await _counselorRepository.UpdateAsync(counselor);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Failed to update status.");

            string action = request.Status == 0 ? "blocked" : "unblocked";
            return ServiceResponse<string>.SuccessResponse($"Counselor {action} successfully.");
        }
    }
}
