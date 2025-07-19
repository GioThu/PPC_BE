using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<bool> IsCourseNameExistAsync(string courseName);
        Task<List<Course>> GetAllCoursesWithDetailsAsync();
    }
}
