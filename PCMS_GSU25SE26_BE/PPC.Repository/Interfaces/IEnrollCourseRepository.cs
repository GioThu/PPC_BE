using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IEnrollCourseRepository : IGenericRepository<EnrollCourse>
    {
        Task<bool> IsEnrolledAsync(string memberId, string courseId);
        Task<List<EnrollCourse>> GetEnrolledCoursesWithProcessingAsync(string memberId);
        Task<EnrollCourse> GetEnrollByCourseAndMemberAsync(string courseId, string memberId);
        Task<bool> OpenCourseForMemberAsync(string courseId, string memberId);
        Task<EnrollCourse> GetByIdWithCourseAsync(string enrollCourseId);
    }
}
