using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelResponse;
using System;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class ProcessingService : IProcessingService
    {
        private readonly IProcessingRepository _processingRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IEnrollCourseRepository _enrollCourseRepository;
        private readonly IMapper _mapper;

        public ProcessingService(
            IProcessingRepository processingRepository,
            IChapterRepository chapterRepository,
            IEnrollCourseRepository enrollCourseRepository,
            IMapper mapper)
        {
            _processingRepository = processingRepository;
            _chapterRepository = chapterRepository;
            _enrollCourseRepository = enrollCourseRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<string>> CreateProcessingAsync(string chapterId, string memberId)
        {
            if (string.IsNullOrEmpty(chapterId) || string.IsNullOrEmpty(memberId))
                return ServiceResponse<string>.ErrorResponse("Thiếu thông tin đầu vào");

            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
                return ServiceResponse<string>.ErrorResponse("Chapter không tồn tại");

            var enroll = await _enrollCourseRepository.GetEnrollByCourseAndMemberAsync(chapter.CourseId, memberId);
            if (enroll == null)
                return ServiceResponse<string>.ErrorResponse("Bạn chưa đăng ký khóa học này");

            var existed = await _processingRepository.IsChapterProcessedAsync(enroll.Id, chapterId);
            if (existed)
                return ServiceResponse<string>.SuccessResponse("Chapter đã được đánh dấu hoàn thành");

            var processing = new Processing
            {
                Id = Utils.Utils.GenerateIdModel("Processing"),
                EnrollCourseId = enroll.Id,
                ChapterId = chapterId,
                CreateDate = Utils.Utils.GetTimeNow(),
                Status = 1
            };

            await _processingRepository.CreateAsync(processing);

            return ServiceResponse<string>.SuccessResponse("Đã đánh dấu chapter là hoàn thành");
        }
    }
}
