using AutoMapper;
using PPC.DAO.Models;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.AccountResponse;
using PPC.Service.ModelResponse.BookingResponse;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CirtificationResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using PPC.Service.ModelResponse.Couple;
using PPC.Service.ModelResponse.CoupleResponse;
using PPC.Service.ModelResponse.CourseResponse;
using PPC.Service.ModelResponse.DepositResponse;
using PPC.Service.ModelResponse.MemberResponse;
using PPC.Service.ModelResponse.PersonTypeResponse;
using PPC.Service.ModelResponse.PostResponse;
using PPC.Service.ModelResponse.SurveyResponse;
using PPC.Service.ModelResponse.WorkScheduleResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PPC.Service.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Account, AccountDto>();
            CreateMap<Course, MemberCourseDto>()
    .ForMember(dest => dest.ChapterCount, opt => opt.MapFrom(src => src.Chapters.Count))
    .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.CourseSubCategories.Select(csc => csc.SubCategory)))
    .ForMember(dest => dest.Chapters, opt => opt.MapFrom(src => src.Chapters))
    .ForMember(dest => dest.ProcessingCount, opt => opt.Ignore());
            CreateMap<Chapter, MemberChapterDto>()
    .ForMember(dest => dest.IsDone, opt => opt.Ignore());
            CreateMap<WorkSchedule, WorkScheduleDto>();
            CreateMap<Category, CategoryDto>();
            CreateMap<Member, MemberDto>();
            CreateMap<Counselor, CounselorDto>();
            CreateMap<SubCategory, SubCategoryDto>();
            CreateMap<Certification, CertificationWithSubDto>();
            CreateMap<Counselor, CounselorWithSubDto>();
            CreateMap<Survey, SurveyDto>();
            CreateMap<Question, SurveyQuestionDto>();
            CreateMap<Answer, SurveyAnswerDto>();
            CreateMap<Post, PostDto>();
            CreateMap<Answer, AnswerDto>();
            CreateMap<PersonType, PersonTypeDto>();
            CreateMap<Deposit, DepositDto>();
            CreateMap<PersonType, MyPersonTypeResponse>();
            CreateMap<Couple, CoupleRoomResponse>();
            CreateMap<Couple, CoupleDetailResponse>()
            .ForMember(dest => dest.Member,
                opt => opt.MapFrom(src => src.MemberNavigation))
            .ForMember(dest => dest.Member1,
                opt => opt.MapFrom(src => src.Member1Navigation));
            CreateMap<ResultPersonType, ResultPersonTypeDto>();
            CreateMap<Chapter, ChapterDetailDto>()
            .ForMember(dest => dest.Lecture, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore())
            .ForMember(dest => dest.Video, opt => opt.Ignore());

            CreateMap<Lecture, LectureDto>();
            CreateMap<Lecture, VideoDto>();
            CreateMap<Quiz, QuizDto>()
    .ForMember(dest => dest.TotalScore, opt => opt.MapFrom(src =>
        src.Questions != null && src.Questions.Any()
            ? src.Questions.Sum(q =>
                q.Answers != null && q.Answers.Any()
                    ? q.Answers.Max(a => a.Score ?? 0)
                    : 0)
            : 0))
    .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src =>
                    src.Answers != null && src.Answers.Any()
                        ? src.Answers.Max(a => a.Score ?? 0)
                        : 0))
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers));


            CreateMap<Course, CourseListDto>()
    .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore())
    .ForMember(dest => dest.FreeByMembershipName, opt => opt.Ignore());

            CreateMap<Chapter, ChapterDto>();
            CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.ChapterCount, opt => opt.MapFrom(src => src.Chapters.Count))
            .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src =>
                src.CourseSubCategories.Select(cs => cs.SubCategory)))
            .ForMember(dest => dest.Chapters, opt => opt.MapFrom(src => src.Chapters));

            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member))      
                .ForMember(dest => dest.Member2, opt => opt.MapFrom(src => src.Member2))    
                .ForMember(dest => dest.Counselor, opt => opt.MapFrom(src => src.Counselor))  
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.BookingSubCategories.Select(bsc => bsc.SubCategory)));

            CreateMap<Booking, BookingAdminResponse>();

            CreateMap<Course, MyCourseDto>()
    .ForMember(dest => dest.SubCategories, opt =>
        opt.MapFrom(src => src.CourseSubCategories.Select(cs => cs.SubCategory)))
    .ForMember(dest => dest.Chapters, opt => opt.MapFrom(src => src.Chapters));

            CreateMap<Booking, BookingDashboardDto>();
            CreateMap<Notification, NotificationDto>();

        }
    }
}
