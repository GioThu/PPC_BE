using PPC.DAO.Models;
using PPC.Service.ModelRequest.CourseRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class LectureMapper
    {
        public static Chapter ToChapter(this LectureWithChapterCreateRequest request, int chapNum)
        {
            return new Chapter
            {
                Id = Utils.Utils.GenerateIdModel("Chapter"),
                CourseId = request.CourseId,
                Name = request.Name,
                Description = request.Description,
                ChapterType = "Lecture",
                ChapNum = chapNum,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1,
            };
        }

        public static Chapter ToChapter(this VideoWithChapterCreateRequest request, int chapNum)
        {
            return new Chapter
            {
                Id = Utils.Utils.GenerateIdModel("Chapter"),
                CourseId = request.CourseId,
                Name = request.Name,
                Description = request.Description,
                ChapterType = "Video",
                ChapNum = chapNum,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1,
            };
        }

        public static Lecture ToLecture(this LectureWithChapterCreateRequest request, string chapterId)
        {
            return new Lecture
            {
                Id = Utils.Utils.GenerateIdModel("Lecture"),
                Name = request.Name,
                Type = "Lecture",
                LectureMetadata = request.LectureMetadata,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1,
            };
        }

        public static Lecture ToLecture(this VideoWithChapterCreateRequest request, string chapterId)
        {
            return new Lecture
            {
                Id = Utils.Utils.GenerateIdModel("Lecture"),
                Name = request.Name,
                Type = "Video",
                VideoUrl = request.VideoUrl,
                TimeVideo = request.TimeVideo,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1,
            };
        }
    }
}
