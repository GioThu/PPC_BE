using PPC.DAO.Models;
using PPC.Service.ModelRequest.CourseRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class QuizMapper
    {
        public static Chapter ToChapter(this QuizWithChapterCreateRequest request, int chapNum)
        {
            return new Chapter
            {
                Id = Utils.Utils.GenerateIdModel("Chapter"),
                CourseId = request.CourseId,
                Description = request.Description,
                Name = request.Name,
                ChapterType = "Quiz",
                ChapNum = chapNum,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1
            };
        }

        public static Quiz ToQuiz(this QuizWithChapterCreateRequest request, string chapterId)
        {
            return new Quiz
            {
                Id = Utils.Utils.GenerateIdModel("Quiz"),
                Name = request.Name,
                Status = 1
            };
        }
    }
}
