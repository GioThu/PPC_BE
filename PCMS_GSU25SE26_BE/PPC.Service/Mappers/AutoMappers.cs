using AutoMapper;
using PPC.DAO.Models;
using PPC.Service.ModelResponse.BookingResponse;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CirtificationResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using PPC.Service.ModelResponse.DepositResponse;
using PPC.Service.ModelResponse.MemberResponse;
using PPC.Service.ModelResponse.PersonTypeResponse;
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
            CreateMap<PersonType, PersonTypeDto>();
            CreateMap<Deposit, DepositDto>();
            CreateMap<PersonType, MyPersonTypeResponse>();



            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member))      
                .ForMember(dest => dest.Member2, opt => opt.MapFrom(src => src.Member2))    
                .ForMember(dest => dest.Counselor, opt => opt.MapFrom(src => src.Counselor))  
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.BookingSubCategories.Select(bsc => bsc.SubCategory)));

            CreateMap<Booking, BookingAdminResponse>();
        }
    }
}
