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
    public class ChapterRepository : GenericRepository<Chapter>, IChapterRepository
    {
        public ChapterRepository(CCPContext context) : base(context) { }
        public async Task<int> GetNextChapterNumberAsync(string courseId)
        {
            var max = await _context.Chapters
                .Where(c => c.CourseId == courseId && c.Status == 1)
                .MaxAsync(c => (int?)c.ChapNum) ?? 0;
            return max + 1;
        }

        public async Task<Chapter> GetByIdAsync(string chapterId)
        {
            return await _context.Chapters.FirstOrDefaultAsync(c => c.Id == chapterId);
        }

        public async Task<List<Chapter>> GetChaptersAfterAsync(string courseId, int chapNum)
        {
            return await _context.Chapters
                .Where(c => c.CourseId == courseId && c.ChapNum > chapNum && c.Status == 1)
                .OrderBy(c => c.ChapNum)
                .ToListAsync();
        }

        public async Task DecreaseChapNumAfterAsync(string courseId, int deletedChapNum)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"UPDATE Chapters
              SET ChapNum = ChapNum - 1
              WHERE CourseId = {0} AND ChapNum > {1}",
                courseId, deletedChapNum
            );
        }

    }
}