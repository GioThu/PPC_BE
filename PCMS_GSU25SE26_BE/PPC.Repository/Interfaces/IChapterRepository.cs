using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IChapterRepository : IGenericRepository<Chapter>
    {
        Task<int> GetNextChapterNumberAsync(string courseId);
        Task<Chapter> GetByIdAsync(string chapterId);
        Task<List<Chapter>> GetChaptersAfterAsync(string courseId, int chapNum);
        Task DecreaseChapNumAfterAsync(string courseId, int deletedChapNum);


    }
}
