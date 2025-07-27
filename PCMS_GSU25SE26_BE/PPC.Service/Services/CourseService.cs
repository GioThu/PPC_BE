using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.CourseRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CourseResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseSubCategoryRepository _courseSubCategoryRepository;
        private readonly IMapper _mapper;
        private readonly ILectureRepository _lectureRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IMemberShipRepository _memberShipRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IEnrollCourseRepository _enrollCourseRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ISysTransactionRepository _sysTransactionRepository;
        private readonly IMemberMemberShipRepository _memberMemberShipRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IProcessingRepository _processingRepository;



        public CourseService(ICourseRepository courseRepository, IMapper mapper, ICourseSubCategoryRepository courseSubCategoryRepository, ILectureRepository lectureRepository, IChapterRepository chapterRepository, IQuizRepository quizRepository, IMemberShipRepository memberShipRepository, IAccountRepository accountRepository, IEnrollCourseRepository enrollCourseRepository, IWalletRepository walletRepository, ISysTransactionRepository sysTransactionRepository, IMemberMemberShipRepository memberMemberShipRepository, IMemberRepository memberRepository, IProcessingRepository processingRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _courseSubCategoryRepository = courseSubCategoryRepository;
            _lectureRepository = lectureRepository;
            _chapterRepository = chapterRepository;
            _quizRepository = quizRepository;
            _memberShipRepository = memberShipRepository;
            _accountRepository = accountRepository;
            _enrollCourseRepository = enrollCourseRepository;
            _walletRepository = walletRepository;
            _sysTransactionRepository = sysTransactionRepository;
            _memberMemberShipRepository = memberMemberShipRepository;
            _memberRepository = memberRepository;
            _processingRepository = processingRepository;
        }

        public async Task<ServiceResponse<string>> CreateCourseAsync(string creatorId, CourseCreateRequest request)
        {
            if (await _courseRepository.IsCourseNameExistAsync(request.Name))
            {
                return ServiceResponse<string>.ErrorResponse("Course name already exists.");
            }

            var course = request.ToCreateCourse();
            course.CreateBy = creatorId;

            await _courseRepository.CreateAsync(course);
            return ServiceResponse<string>.SuccessResponse("Course created successfully.");
        }

        public async Task<ServiceResponse<List<CourseDto>>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetAllCoursesWithDetailsAsync();

            var courseDtos = courses.Select(c => c.ToDto()).ToList();
            return ServiceResponse<List<CourseDto>>.SuccessResponse(courseDtos);
        }

        public async Task<ServiceResponse<string>> AddSubCategoryAsync(CourseSubCategoryRequest request)
        {
            if (await _courseSubCategoryRepository.ExistsAsync(request.CourseId, request.SubCategoryId))
            {
                return ServiceResponse<string>.ErrorResponse("Sub-category already exists in course.");
            }

            var entry = new CourseSubCategory
            {
                Id = Utils.Utils.GenerateIdModel("CourseSubCategory"),
                CourseId = request.CourseId,
                SubCategoryId = request.SubCategoryId
            };

            await _courseSubCategoryRepository.CreateAsync(entry);
            return ServiceResponse<string>.SuccessResponse("Sub-category added to course successfully.");
        }

        public async Task<ServiceResponse<string>> RemoveSubCategoryAsync(CourseSubCategoryRequest request)
        {
            var entry = await _courseSubCategoryRepository.GetAsync(request.CourseId, request.SubCategoryId);
            if (entry == null)
            {
                return ServiceResponse<string>.ErrorResponse("Sub-category not found in course.");
            }

            await _courseSubCategoryRepository.RemoveAsync(entry);
            return ServiceResponse<string>.SuccessResponse("Sub-category removed from course successfully.");
        }

        public async Task<ServiceResponse<string>> CreateLectureWithChapterAsync(LectureWithChapterCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ServiceResponse<string>.ErrorResponse("Name is required.");

            var nextChapNum = await _chapterRepository.GetNextChapterNumberAsync(request.CourseId);

            var chapter = request.ToChapter(nextChapNum);
            await _chapterRepository.CreateAsync(chapter);


            var lecture = request.ToLecture(chapter.Id);
            await _lectureRepository.CreateAsync(lecture);
            chapter.ChapNo = lecture.Id;

            await _chapterRepository.UpdateAsync(chapter);

            return ServiceResponse<string>.SuccessResponse("Lecture and chapter created successfully.");
        }

        public async Task<ServiceResponse<string>> CreateVideoWithChapterAsync(VideoWithChapterCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ServiceResponse<string>.ErrorResponse("Name is required.");

            var nextChapNum = await _chapterRepository.GetNextChapterNumberAsync(request.CourseId);

            var chapter = request.ToChapter(nextChapNum);
            await _chapterRepository.CreateAsync(chapter);


            var lecture = request.ToLecture(chapter.Id);
            await _lectureRepository.CreateAsync(lecture);
            chapter.ChapNo = lecture.Id;

            await _chapterRepository.UpdateAsync(chapter);

            return ServiceResponse<string>.SuccessResponse("Lecture and chapter created successfully.");
        }

        public async Task<ServiceResponse<string>> CreateQuizWithChapterAsync(QuizWithChapterCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ServiceResponse<string>.ErrorResponse("Name is required.");

            var nextChapNum = await _chapterRepository.GetNextChapterNumberAsync(request.CourseId);
            var chapter = request.ToChapter(nextChapNum);
            await _chapterRepository.CreateAsync(chapter);

            var quiz = request.ToQuiz(chapter.Id);
            await _quizRepository.CreateAsync(quiz);
            chapter.ChapNo = quiz.Id;
            await _chapterRepository.UpdateAsync(chapter);

            return ServiceResponse<string>.SuccessResponse("Quiz and Chapter created successfully.");
        }

        public async Task<ServiceResponse<CourseDto>> GetCourseDetailByIdAsync(string courseId)
        {
            var course = await _courseRepository.GetCourseWithAllDetailsAsync(courseId);
            if (course == null)
                return ServiceResponse<CourseDto>.ErrorResponse("Course not found.");

            var dto = _mapper.Map<CourseDto>(course);
            return ServiceResponse<CourseDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<ChapterDetailDto>> GetChapterDetailAsync(string chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
                return ServiceResponse<ChapterDetailDto>.ErrorResponse("Chapter not found.");

            var dto = _mapper.Map<ChapterDetailDto>(chapter);

            if (chapter.ChapterType == "Lecture")
            {
                var lecture = await _lectureRepository.GetByIdAsync(chapter.ChapNo);

                dto.Lecture = _mapper.Map<LectureDto>(lecture);
            }
            else if (chapter.ChapterType == "Quiz")
            {
                var quiz = await _quizRepository.GetByIdWithDetailsAsync(chapter.ChapNo);
                dto.Quiz = _mapper.Map<QuizDto>(quiz);
            }
            else if (chapter.ChapterType == "Video")
            {
                var video = await _lectureRepository.GetByIdAsync(chapter.ChapNo);
                dto.Video = _mapper.Map<VideoDto>(video);
            }

            return ServiceResponse<ChapterDetailDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<List<CourseListDto>>> GetAllCoursesAsync(string accountId, string memberId)
        {
            var courses = await _courseRepository.GetAllActiveCoursesAsync();
            var courseDtos = _mapper.Map<List<CourseListDto>>(courses);

            var enrollCourses = await _courseRepository.GetEnrollCoursesByAccountIdAsync(memberId);

            var activeMemberships = await _memberShipRepository.GetActiveMemberShipsByMemberIdAsync(memberId);

            var allMemberships = await _memberShipRepository.GetAllActiveAsync();
            var rankToMembershipName = allMemberships
                .Where(m => m.Rank.HasValue)
                .ToDictionary(m => m.Rank.Value, m => m.MemberShipName);

            var memberMaxRank = activeMemberships
                .Where(m => m.MemberShip?.Rank != null)
                .Select(m => m.MemberShip.Rank.Value)
                .DefaultIfEmpty(0)
                .Max();

            foreach (var dto in courseDtos)
            {
                var enrolled = enrollCourses.FirstOrDefault(e => e.CourseId == dto.Id);

                dto.IsEnrolled = enrolled?.IsOpen == true;
                dto.IsBuy = enrolled?.Status == 0 || enrolled?.Status == 1;

                dto.IsFree = dto.Rank.HasValue && memberMaxRank >= dto.Rank.Value;

                if (dto.Rank.HasValue && rankToMembershipName.TryGetValue(dto.Rank.Value, out var name))
                {
                    dto.FreeByMembershipName = name;
                }
            }

            return ServiceResponse<List<CourseListDto>>.SuccessResponse(courseDtos);
        }

        public async Task<ServiceResponse<EnrollCourseResultDto>> EnrollCourseAsync(string courseId, string accountId)
        {
            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(courseId))
                return ServiceResponse<EnrollCourseResultDto>.ErrorResponse("Thiếu thông tin đầu vào.");

            // Lấy tài khoản
            var account = await _accountRepository.GetAccountWithWalletAsync(accountId);
            if (account == null || string.IsNullOrEmpty(account.WalletId))
                return ServiceResponse<EnrollCourseResultDto>.ErrorResponse("Tài khoản không hợp lệ hoặc không có ví.");

            // Lấy ví
            var wallet = account.Wallet;
            if (wallet == null || wallet.Status != 1)
                return ServiceResponse<EnrollCourseResultDto>.ErrorResponse("Ví không khả dụng.");

            // Lấy member
            var member = await _memberRepository.GetByAccountIdAsync(account.Id);
            if (member == null)
                return ServiceResponse<EnrollCourseResultDto>.ErrorResponse("Không tìm thấy thành viên.");

            // Kiểm tra đã đăng ký khóa học chưa
            var isAlreadyEnrolled = await _enrollCourseRepository.IsEnrolledAsync(member.Id, courseId);
            if (isAlreadyEnrolled)
                return ServiceResponse<EnrollCourseResultDto>.ErrorResponse("Bạn đã đăng ký khóa học này rồi.");

            // Lấy khóa học
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null || course.Status == 0)
                return ServiceResponse<EnrollCourseResultDto>.ErrorResponse("Không tìm thấy khóa học.");

            // Lấy danh sách MemberShip còn hạn
            var activeMemberships = await _memberShipRepository.GetActiveMemberShipsByMemberIdAsync(member.Id);

            // Kiểm tra miễn phí
            bool isFree = activeMemberships.Any(ms => ms.MemberShip.Rank >= course.Rank);
            double coursePrice = course.Price ?? 0;
            double finalPrice = 0;

            if (!isFree)
            {
                int maxDiscount = activeMemberships
                    .Select(ms => ms.MemberShip.DiscountCourse ?? 0)
                    .DefaultIfEmpty(0)
                    .Max();

                finalPrice = Math.Round(coursePrice * (1 - maxDiscount / 100.0), 0);
            }

            if (!isFree && (wallet.Remaining ?? 0) < finalPrice)
                return ServiceResponse<EnrollCourseResultDto>.ErrorResponse("Số dư trong ví không đủ để đăng ký khóa học.");

            // Tạo EnrollCourse
            var enroll = new EnrollCourse
            {
                Id = Utils.Utils.GenerateIdModel("EnrollCourse"),
                CourseId = courseId,
                MemberId = member.Id,
                CreateDate = Utils.Utils.GetTimeNow(),
                Price = finalPrice,
                Status = 1,
                Processing = 0,
                IsOpen = false,
            };
            await _enrollCourseRepository.CreateAsync(enroll);

            // Nếu không free thì trừ tiền + tạo giao dịch
            string transactionId = null;
            if (finalPrice > 0)
            {
                wallet.Remaining -= finalPrice;
                await _walletRepository.UpdateAsync(wallet);

                var transaction = new SysTransaction
                {
                    Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                    TransactionType = "4", // enroll course
                    DocNo = enroll.Id,
                    CreateBy = accountId,
                    CreateDate = Utils.Utils.GetTimeNow()
                };
                await _sysTransactionRepository.CreateAsync(transaction);

                transactionId = transaction.Id;
            }

            return ServiceResponse<EnrollCourseResultDto>.SuccessResponse(new EnrollCourseResultDto
            {
                EnrollCourseId = enroll.Id,
                PaidAmount = finalPrice,
                Remaining = wallet.Remaining,
                TransactionId = transactionId,
                Message = "Đăng ký khóa học thành công."
            });
        }

        public async Task<ServiceResponse<List<MyCourseDto>>> GetEnrolledCoursesWithProgressAsync(string memberId)
        {
            var enrolls = await _enrollCourseRepository.GetEnrolledCoursesWithProcessingAsync(memberId);

            var courseDtos = new List<MyCourseDto>();

            foreach (var enroll in enrolls)
            {
                var course = enroll.Course;

                if (course != null)
                {
                    var dto = _mapper.Map<MyCourseDto>(course);
                    dto.ChapterCount = course.Chapters?.Count ?? 0;
                    dto.ProcessingCount = enroll.Processings?.Count ?? 0;
                    dto.IsOpen = enroll.IsOpen ?? false;
                    courseDtos.Add(dto);
                }
            }

            return ServiceResponse<List<MyCourseDto>>.SuccessResponse(courseDtos);
        }

        public async Task<ServiceResponse<string>> UpdateCourseAsync(string courseId, CourseUpdateRequest request)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                return ServiceResponse<string>.ErrorResponse("Course not found");

            course.Name = request.Name?.Trim();
            course.Thumble = request.Thumble;
            course.Description = request.Description;
            course.Price = request.Price;
            course.Rank = request.Rank;
            course.UpdateAt = Utils.Utils.GetTimeNow();

            var result = await _courseRepository.UpdateAsync(course);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Update failed");

            return ServiceResponse<string>.SuccessResponse("Course updated successfully");
        }

        public async Task<ServiceResponse<MemberCourseDto>> GetMemberCourseDetailAsync(string courseId, string memberId)
        {
            // Kiểm tra EnrollCourse
            var enroll = await _enrollCourseRepository.GetEnrollByCourseAndMemberAsync(courseId, memberId);
            if (enroll == null)
                return ServiceResponse<MemberCourseDto>.ErrorResponse("Bạn chưa học khóa học này");

            // Lấy dữ liệu Course
            var course = await _courseRepository.GetCourseWithAllDetailsAsync(courseId);
            if (course == null)
                return ServiceResponse<MemberCourseDto>.ErrorResponse("Course không tồn tại");

            // Map sang DTO
            var dto = _mapper.Map<MemberCourseDto>(course);

            // Gán ChapterCount từ source
            dto.ChapterCount = course.Chapters?.Count ?? 0;

            var doneChapterIds = await _processingRepository.GetProcessingChapterIdsByEnrollCourseIdAsync(enroll.Id);
            var doneSet = doneChapterIds.ToHashSet();

            // Bảo vệ null cho Chapters
            dto.Chapters ??= new List<MemberChapterDto>();

            foreach (var chapter in dto.Chapters)
            {
                chapter.IsDone = doneSet.Contains(chapter.Id);
            }

            // Gán số lượng đã làm
            dto.ProcessingCount = doneSet.Count;

            return ServiceResponse<MemberCourseDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<string>> UpdateLectureByChapterIdAsync(UpdateLectureRequest request)
        {
            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
                return ServiceResponse<string>.ErrorResponse("Chapter không tồn tại.");

            if (chapter.ChapterType != "Lecture")
                return ServiceResponse<string>.ErrorResponse("Chapter này không phải loại Lecture.");

            if (!string.IsNullOrEmpty(request.ChapterName))
                chapter.Name = request.ChapterName;
            if (!string.IsNullOrEmpty(request.ChapterDescription))
                chapter.Description = request.ChapterDescription;

            await _chapterRepository.UpdateAsync(chapter);

            if (!string.IsNullOrEmpty(chapter.ChapNo))
            {
                var lecture = await _lectureRepository.GetByIdAsync(chapter.ChapNo);
                if (lecture != null && !string.IsNullOrEmpty(request.LectureMetadata))
                {
                    lecture.LectureMetadata = request.LectureMetadata;
                    await _lectureRepository.UpdateAsync(lecture);
                }
            }

            return ServiceResponse<string>.SuccessResponse("Cập nhật Lecture thành công.");
        }

        public async Task<ServiceResponse<string>> UpdateVideoByChapterIdAsync(UpdateVideoRequest request)
        {
            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
                return ServiceResponse<string>.ErrorResponse("Chapter không tồn tại.");

            if (chapter.ChapterType != "Video")
                return ServiceResponse<string>.ErrorResponse("Chapter này không phải loại Video.");

            if (!string.IsNullOrEmpty(request.ChapterName))
                chapter.Name = request.ChapterName;
            if (!string.IsNullOrEmpty(request.ChapterDescription))
                chapter.Description = request.ChapterDescription;

            await _chapterRepository.UpdateAsync(chapter);

            if (!string.IsNullOrEmpty(chapter.ChapNo))
            {
                var video = await _lectureRepository.GetByIdAsync(chapter.ChapNo);
                if (video != null)
                {
                    if (!string.IsNullOrEmpty(request.VideoUrl))
                        video.VideoUrl = request.VideoUrl;
                    if (request.TimeVideo.HasValue)
                        video.TimeVideo = request.TimeVideo;

                    await _lectureRepository.UpdateAsync(video);
                }
            }

            return ServiceResponse<string>.SuccessResponse("Cập nhật Video thành công.");
        }

        public async Task<ServiceResponse<string>> UpdateQuizByChapterIdAsync(UpdateQuizRequest request)
        {
            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
                return ServiceResponse<string>.ErrorResponse("Chapter không tồn tại.");

            if (chapter.ChapterType != "Quiz")
                return ServiceResponse<string>.ErrorResponse("Chapter này không phải loại Quiz.");

            if (!string.IsNullOrEmpty(request.ChapterName))
                chapter.Name = request.ChapterName;

            if (!string.IsNullOrEmpty(request.ChapterDescription))
                chapter.Description = request.ChapterDescription;

            await _chapterRepository.UpdateAsync(chapter);

            return ServiceResponse<string>.SuccessResponse("Cập nhật Quiz thành công.");
        }

        public async Task<ServiceResponse<string>> OpenCourseAsync(string courseId, string memberId)
        {
            var success = await _enrollCourseRepository.OpenCourseForMemberAsync(courseId, memberId);

            if (!success)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy dữ liệu để mở khóa học.");

            return ServiceResponse<string>.SuccessResponse("Đã mở khóa học thành công.");
        }

        public async Task<ServiceResponse<string>> ChangeCourseStatusAsync(string courseId, int newStatus)
        {
            // Validate input status
            if (newStatus < 0 || newStatus > 2)
                return ServiceResponse<string>.ErrorResponse("Invalid status. Must be 0, 1 or 2.");

            // Get course by id
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                return ServiceResponse<string>.ErrorResponse("Course not found.");

            // Update status
            course.Status = newStatus;
            course.UpdateAt = Utils.Utils.GetTimeNow();

            await _courseRepository.UpdateAsync(course);

            return ServiceResponse<string>.SuccessResponse("Course status updated successfully.");
        }

        public async Task<ServiceResponse<string>> DeleteChapterAsync(string chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
                return ServiceResponse<string>.ErrorResponse("Chapter không tồn tại.");

            chapter.Status = 0;
            await _chapterRepository.UpdateAsync(chapter);

            var affectedChapters = await _chapterRepository.GetChaptersAfterAsync(chapter.CourseId, chapter.ChapNum ?? 0);

            foreach (var ch in affectedChapters)
            {
                ch.ChapNum = ch.ChapNum - 1;
                await _chapterRepository.UpdateAsync(ch);
            }

            return ServiceResponse<string>.SuccessResponse("Xóa chapter thành công và cập nhật thứ tự.");
        }
    }
}