using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.CourseRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CategoryResponse;
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
        private readonly ICoupleRepository _coupleRepository;
        private readonly IMemberShipService _memberShipService;



        public CourseService(ICourseRepository courseRepository, IMapper mapper, ICourseSubCategoryRepository courseSubCategoryRepository, ILectureRepository lectureRepository, IChapterRepository chapterRepository, IQuizRepository quizRepository, IMemberShipRepository memberShipRepository, IAccountRepository accountRepository, IEnrollCourseRepository enrollCourseRepository, IWalletRepository walletRepository, ISysTransactionRepository sysTransactionRepository, IMemberMemberShipRepository memberMemberShipRepository, IMemberRepository memberRepository, IProcessingRepository processingRepository, ICoupleRepository coupleRepository, IMemberShipService memberShipService)
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
            _coupleRepository = coupleRepository;
            _memberShipService = memberShipService;
        }

        public async Task<ServiceResponse<string>> CreateCourseAsync(string creatorId, CourseCreateRequest request)
        {
            if (await _courseRepository.IsCourseNameExistAsync(request.Name))
            {
                return ServiceResponse<string>.ErrorResponse("Tên khóa học đã tồn tại.");
            }

            var course = request.ToCreateCourse();
            course.CreateBy = creatorId;

            await _courseRepository.CreateAsync(course);
            return ServiceResponse<string>.SuccessResponse("Khóa học đã được tạo thành công.");
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
                return ServiceResponse<string>.ErrorResponse("Danh mục phụ đã tồn tại trong khóa học.");
            }

            var entry = new CourseSubCategory
            {
                Id = Utils.Utils.GenerateIdModel("CourseSubCategory"),
                CourseId = request.CourseId,
                SubCategoryId = request.SubCategoryId
            };

            await _courseSubCategoryRepository.CreateAsync(entry);
            return ServiceResponse<string>.SuccessResponse("Danh mục phụ đã được thêm vào khóa học thành công");
        }

        public async Task<ServiceResponse<string>> RemoveSubCategoryAsync(CourseSubCategoryRequest request)
        {
            var entry = await _courseSubCategoryRepository.GetAsync(request.CourseId, request.SubCategoryId);
            if (entry == null)
            {
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy danh mục phụ trong khóa học");
            }

            await _courseSubCategoryRepository.RemoveAsync(entry);
            return ServiceResponse<string>.SuccessResponse("Danh mục phụ đã được xóa khỏi khóa học thành công.");
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

            return ServiceResponse<string>.SuccessResponse("Đã tạo Lecture thành công");
        }

        public async Task<ServiceResponse<string>> CreateVideoWithChapterAsync(VideoWithChapterCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ServiceResponse<string>.ErrorResponse("Thiếu tên của video");

            var nextChapNum = await _chapterRepository.GetNextChapterNumberAsync(request.CourseId);

            var chapter = request.ToChapter(nextChapNum);
            await _chapterRepository.CreateAsync(chapter);


            var lecture = request.ToLecture(chapter.Id);
            await _lectureRepository.CreateAsync(lecture);
            chapter.ChapNo = lecture.Id;

            await _chapterRepository.UpdateAsync(chapter);

            return ServiceResponse<string>.SuccessResponse("Đã tạo lecture thành công");
        }

        public async Task<ServiceResponse<string>> CreateQuizWithChapterAsync(QuizWithChapterCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ServiceResponse<string>.ErrorResponse("Cần phải có tên");

            var nextChapNum = await _chapterRepository.GetNextChapterNumberAsync(request.CourseId);
            var chapter = request.ToChapter(nextChapNum);
            await _chapterRepository.CreateAsync(chapter);

            var quiz = request.ToQuiz(chapter.Id);
            await _quizRepository.CreateAsync(quiz);
            chapter.ChapNo = quiz.Id;
            await _chapterRepository.UpdateAsync(chapter);

            return ServiceResponse<string>.SuccessResponse("Đã tạo quiz thành công");
        }

        public async Task<ServiceResponse<CourseDto>> GetCourseDetailByIdAsync(string courseId)
        {
            var course = await _courseRepository.GetCourseWithAllDetailsAsync(courseId);
            if (course == null)
                return ServiceResponse<CourseDto>.ErrorResponse("Không tìm thấy khóa học");

            var dto = _mapper.Map<CourseDto>(course);
            return ServiceResponse<CourseDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<ChapterDetailDto>> GetChapterDetailAsync(string chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
                return ServiceResponse<ChapterDetailDto>.ErrorResponse("Không tìm thấy chapter");

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
            
                wallet.Remaining -= finalPrice;
                await _walletRepository.UpdateAsync(wallet);

                var transaction = new SysTransaction
                {
                    Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                    TransactionType = "4",
                    DocNo = enroll.Id,
                    CreateBy = accountId,
                    CreateDate = Utils.Utils.GetTimeNow()
                };
                await _sysTransactionRepository.CreateAsync(transaction);

                transactionId = transaction.Id;
            

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

                    var validChapters = course.Chapters?
                        .Where(c => c.Status == 1)
                        .ToList() ?? new List<Chapter>();

                    dto.ChapterCount = validChapters.Count;

                    var validChapterIds = validChapters.Select(c => c.Id).ToHashSet();

                    dto.ProcessingCount = enroll.Processings?
                        .Count(p => p.Status == 1 && validChapterIds.Contains(p.ChapterId)) ?? 0;

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
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy khóa học");

            course.Name = request.Name?.Trim();
            course.Thumble = request.Thumble;
            course.Description = request.Description;
            course.Price = request.Price;
            course.Rank = request.Rank;
            course.UpdateAt = Utils.Utils.GetTimeNow();

            var result = await _courseRepository.UpdateAsync(course);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Cập nhật thất bại");

            return ServiceResponse<string>.SuccessResponse("Cập nhật khóa học thành công");
        }

        public async Task<ServiceResponse<MemberCourseDto>> GetMemberCourseDetailAsync(string courseId, string memberId)
        {
            // Lấy thông tin enroll của member với course
            var enroll = await _enrollCourseRepository.GetEnrollByCourseAndMemberAsync(courseId, memberId);

            // Lấy dữ liệu course với tất cả details
            var course = await _courseRepository.GetCourseWithAllDetailsAsync(courseId);
            if (course == null)
                return ServiceResponse<MemberCourseDto>.ErrorResponse("Course không tồn tại");

            // Map sang DTO
            var dto = _mapper.Map<MemberCourseDto>(course);

            // Tổng số chapter
            dto.ChapterCount = course.Chapters?.Count(c => c.Status == 1) ?? 0;

            // Xử lý Processing
            if (enroll != null)
            {
                var doneChapterIds = await _processingRepository.GetProcessingChapterIdsByEnrollCourseIdAsync(enroll.Id);
                var doneSet = doneChapterIds.ToHashSet();

                dto.Chapters ??= new List<MemberChapterDto>();

                foreach (var chapter in dto.Chapters)
                {
                    chapter.IsDone = doneSet.Contains(chapter.Id);
                }

                dto.ProcessingCount = doneSet.Count;
            }
            else
            {
                dto.ProcessingCount = 0;
                dto.Chapters ??= new List<MemberChapterDto>();
                foreach (var chapter in dto.Chapters)
                {
                    chapter.IsDone = false;
                }
            }

            // ===== Xử lý IsEnrolled, IsBuy, IsFree =====
            dto.IsEnrolled = enroll?.IsOpen == true;
            dto.IsBuy = enroll != null && (enroll.Status == 0 || enroll.Status == 1);

            // Lấy rank member để check free course
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

            dto.IsFree = dto.Rank.HasValue && memberMaxRank >= dto.Rank.Value;

            if (dto.Rank.HasValue && rankToMembershipName.TryGetValue(dto.Rank.Value, out var name))
            {
                dto.FreeByMembershipName = name;
            }

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
                return ServiceResponse<string>.ErrorResponse("Trạng thái không hợp lệ. Phải là 0, 1 hoặc 2");

            // Get course by id
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy khóa học");

            // Update status
            course.Status = newStatus;
            course.UpdateAt = Utils.Utils.GetTimeNow();

            await _courseRepository.UpdateAsync(course);

            return ServiceResponse<string>.SuccessResponse("Trạng thái khóa học đã được cập nhật thành công");
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

        public async Task<ServiceResponse<string>> RateCourseAsync(string courseId, string memberId, int rating, string feedback)
        {
            var success = await _courseRepository.RateCourseAsync(courseId, memberId, rating, feedback);

            if (!success)
                return ServiceResponse<string>.ErrorResponse("Thành viên chưa đăng ký khóa học hoặc đã xảy ra lỗi");

            return ServiceResponse<string>.SuccessResponse("Khóa học đã được đánh giá thành công");
        }

        public async Task<ServiceResponse<List<ReviewDto>>> GetCourseReviewsAsync(string courseId)
        {
            var enrollCourses = await _courseRepository.GetEnrollCoursesByCourseIdAsync(courseId);

            if (enrollCourses == null || !enrollCourses.Any())
                return ServiceResponse<List<ReviewDto>>.ErrorResponse("Không có đánh giá nào cho khóa học này");

            var reviews = enrollCourses.Select(e => new ReviewDto
            {
                Rating = e.Rating,
                Feedback = e.Feedback,
                MemberName = e.Member.Fullname 
            }).ToList();

            return ServiceResponse<List<ReviewDto>>.SuccessResponse(reviews);
        }

        public async Task<ServiceResponse<List<CourseWithSubCategoryDto>>> GetRecommendedCoursesAsync(string memberId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
                return ServiceResponse<List<CourseWithSubCategoryDto>>.ErrorResponse("Member not found.");

            // Extract preferred categories
            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(member.Rec1)) categoryIds.Add(member.Rec1);
            if (!string.IsNullOrEmpty(member.Rec2)) categoryIds.Add(member.Rec2);

            List<Course> recommendedCourses = categoryIds.Any()
                ? await _courseRepository.GetCoursesByCategoriesAsync(categoryIds)
                : await _courseRepository.GetTopRatedCoursesAsync(5);

            var rankedCourses = recommendedCourses
                .Select(course =>
                {
                    var subCategories = course.CourseSubCategories?
                        .Where(cs =>
                            cs.SubCategory != null &&
                            cs.SubCategory.Status == 1 &&
                            cs.SubCategory.Category != null &&
                            cs.SubCategory.Category.Status == 1 &&
                            (categoryIds.Count == 0 || categoryIds.Contains(cs.SubCategory.CategoryId)))
                        .GroupBy(sc => sc.SubCategory.Id)
                        .Select(g => new SubCategoryDto
                        {
                            Id = g.Key,
                            Name = g.First().SubCategory.Name
                        })
                        .ToList() ?? new List<SubCategoryDto>();

                    int matchedSubCategories = subCategories.Count;

                    double rating = course.Rating ?? 0.0;   // null safe
                    double reviewsCount = course.Reviews ?? 0.0; // null safe

                    double score = rating * Math.Max(1, Math.Log(reviewsCount + 1)) * (1 + matchedSubCategories);

                    return new
                    {
                        Course = course,
                        SubCategories = subCategories,
                        Score = score
                    };
                })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => new CourseWithSubCategoryDto
                {
                    Id = x.Course.Id,
                    Name = x.Course.Name,
                    Thumble = x.Course.Thumble,
                    Description = x.Course.Description,
                    Price = x.Course.Price,
                    Rating = x.Course.Rating ?? 0.0,
                    Reviews = x.Course.Reviews ?? 0,
                    SubCategories = x.SubCategories
                })
                .ToList();

            if (!rankedCourses.Any())
            {
                rankedCourses = await GetTopRatedCoursesFallbackAsync();
            }

            return ServiceResponse<List<CourseWithSubCategoryDto>>.SuccessResponse(rankedCourses);
        }

        public async Task<ServiceResponse<List<CourseWithSubCategoryDto>>> GetRecommendedCoursesByCoupleIdAsync(string coupleId)
        {
            var couple = await _coupleRepository.GetByIdAsync(coupleId);
            if (couple == null)
                return ServiceResponse<List<CourseWithSubCategoryDto>>.ErrorResponse("Couple not found.");

            Member member = null;
            if (!string.IsNullOrWhiteSpace(couple.Member))
            {
                member = await _memberRepository.GetByIdAsync(couple.Member);
            }

            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(couple.Rec1)) categoryIds.Add(couple.Rec1);
            if (!string.IsNullOrEmpty(member?.Rec2)) categoryIds.Add(member.Rec2);

            List<Course> recommendedCourses = categoryIds.Any()
                ? await _courseRepository.GetCoursesByCategoriesAsync(categoryIds)
                : await _courseRepository.GetTopRatedCoursesAsync(5);

            var rankedCourses = recommendedCourses
                .Select(course =>
                {
                    var subCategories = course.CourseSubCategories?
                        .Where(cs =>
                            cs.SubCategory != null &&
                            cs.SubCategory.Status == 1 &&
                            cs.SubCategory.Category != null &&
                            cs.SubCategory.Category.Status == 1 &&
                            (categoryIds.Count == 0 || categoryIds.Contains(cs.SubCategory.CategoryId)))
                        .GroupBy(sc => sc.SubCategory.Id)
                        .Select(g => new SubCategoryDto
                        {
                            Id = g.Key,
                            Name = g.First().SubCategory.Name
                        })
                        .ToList() ?? new List<SubCategoryDto>();

                    int matchedSubCategories = (categoryIds.Count == 0) ? 0 : subCategories.Count;

                    double rating = course.Rating ?? 0.0;
                    double reviewsCount = course.Reviews ?? 0.0;

                    double score = rating * Math.Max(1, Math.Log(reviewsCount + 1)) * (1 + matchedSubCategories);

                    return new
                    {
                        Course = course,
                        SubCategories = subCategories,
                        Score = score
                    };
                })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => new CourseWithSubCategoryDto
                {
                    Id = x.Course.Id,
                    Name = x.Course.Name,
                    Thumble = x.Course.Thumble,
                    Description = x.Course.Description,
                    Price = x.Course.Price,
                    Rating = x.Course.Rating ?? 0.0,
                    Reviews = x.Course.Reviews ?? 0,
                    SubCategories = x.SubCategories
                })
                .ToList();

            if (!rankedCourses.Any())
            {
                rankedCourses = await GetTopRatedCoursesFallbackAsync();
            }

            return ServiceResponse<List<CourseWithSubCategoryDto>>.SuccessResponse(rankedCourses);
        }

        public async Task<ServiceResponse<int>> GetMaxCourseDiscountByMemberWrappedAsync(string memberId)
        {
            var discount = await _memberShipService.GetMaxCourseDiscountByMemberAsync(memberId);
            return ServiceResponse<int>.SuccessResponse(discount);
        }


        private async Task<List<CourseWithSubCategoryDto>> GetTopRatedCoursesFallbackAsync()
        {
            var courses = await _courseRepository.GetTopRatedCoursesAsync(5);

            var result = courses.Select(course => new CourseWithSubCategoryDto
            {
                Id = course.Id,
                Name = course.Name,
                Thumble = course.Thumble,
                Description = course.Description,
                Price = course.Price,
                Rating = course.Rating,
                Reviews = course.Reviews,
                SubCategories = course.CourseSubCategories
                    .Where(cs => cs.SubCategory.Status == 1 && cs.SubCategory.Category.Status == 1)
                    .GroupBy(sc => sc.SubCategory.Id)
                    .Select(g => new SubCategoryDto
                    {
                        Id = g.Key,
                        Name = g.First().SubCategory.Name
                    })
                    .ToList()
            }).ToList();

            return result;
        }
    }
}