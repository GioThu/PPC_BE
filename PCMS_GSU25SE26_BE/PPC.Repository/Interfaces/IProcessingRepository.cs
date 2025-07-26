using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IProcessingRepository : IGenericRepository<Processing>
    {
        Task<List<string>> GetProcessingChapterIdsByEnrollCourseIdAsync(string enrollCourseId);
        Task<bool> IsChapterProcessedAsync(string enrollCourseId, string chapterId);
    }
}
