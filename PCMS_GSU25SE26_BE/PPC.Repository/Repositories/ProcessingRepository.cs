using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using PPC.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Repositories
{
    public class ProcessingRepository : GenericRepository<Processing>, IProcessingRepository
    {
        public ProcessingRepository(CCPContext context) : base(context)
        {
        }

        public async Task<List<string>> GetProcessingChapterIdsByEnrollCourseIdAsync(string enrollCourseId)
        {
            return await _context.Processings
                .Where(p => p.EnrollCourseId == enrollCourseId
                            && p.Status == 1
                            && p.Chapter.Status == 1) 
                .Select(p => p.ChapterId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> IsChapterProcessedAsync(string enrollCourseId, string chapterId)
        {
            return await _context.Processings
                .AnyAsync(p => p.EnrollCourseId == enrollCourseId &&
                               p.ChapterId == chapterId &&
                               p.Status == 1);
        }
    }
}
