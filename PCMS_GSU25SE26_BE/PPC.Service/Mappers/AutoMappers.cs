using AutoMapper;
using PPC.DAO.Models;
using PPC.Service.ModelResponse;
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
        }
    }
}
