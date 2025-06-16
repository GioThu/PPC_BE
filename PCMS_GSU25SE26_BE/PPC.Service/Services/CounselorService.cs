using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest;
using PPC.Service.ModelResponse;
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
                    .Select(csc => _mapper.Map<SubCategoryDto>(csc.SubCategory))
                    .ToList();

                dto.SubCategories = subCategories;
                return dto;
            }).ToList();

            return ServiceResponse<List<CounselorWithSubDto>>.SuccessResponse(result);
        }

        public async Task<ServiceResponse<AvailableScheduleDto>> GetAvailableScheduleAsync(GetAvailableScheduleRequest request)
        {
            var workSchedules = await _workScheduleRepo.GetByCounselorAndDateAsync(request.CounselorId, request.WorkDate);

            var bookings = await _bookingRepo.GetConfirmedBookingsByDateAsync(request.CounselorId, request.WorkDate);

            var subCategories = await _counselorSubCategoryRepository
                .GetApprovedSubCategoriesByCounselorAsync(request.CounselorId);

            // ✅ Áp dụng buffer 10 phút SAU booking
            var bookingIntervals = bookings
                .Where(b => b.TimeStart.HasValue && b.TimeEnd.HasValue)
                .Select(b => new
                {
                    Start = b.TimeStart.Value.TimeOfDay,
                    End = b.TimeEnd.Value.TimeOfDay + TimeSpan.FromMinutes(10)
                })
                .OrderBy(b => b.Start)
                .ToList();

            var availableSlots = new List<AvailableTimeSlotDto>();

            foreach (var schedule in workSchedules)
            {
                if (!schedule.StartTime.HasValue || !schedule.EndTime.HasValue)
                    continue;

                var currentStart = schedule.StartTime.Value.TimeOfDay;
                var scheduleEnd = schedule.EndTime.Value.TimeOfDay;

                foreach (var b in bookingIntervals)
                {
                    if (b.End <= currentStart)
                        continue;

                    if (b.Start >= scheduleEnd)
                        break;

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

            var subCategoryDtos = _mapper.Map<List<SubCategoryDto>>(subCategories);

            availableSlots = availableSlots
                .OrderBy(slot => slot.Start)
                .ToList();

            return ServiceResponse<AvailableScheduleDto>.SuccessResponse(new AvailableScheduleDto
            {
                CounselorId = request.CounselorId,
                WorkDate = request.WorkDate.Date,
                AvailableSlots = availableSlots,
                SubCategories = subCategoryDtos
            });
        }
    }
}
