using PPC.DAO.Models;
using PPC.Service.ModelRequest.SurveyRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class QuestionMapper
    {
        public static Question ToCreateSurveyQuestion(this SurveyQuestionCreateRequest request)
        {
            return new Question
            {
                Id = Utils.Utils.GenerateIdModel("Question"),
                SurveyId = request.SurveyId,
                Description = request.Description,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1
            };
        }

        public static List<Answer> ToCreateSurveyAnswers(this List<SurveyAnswerCreateRequest> requests, string questionId)
        {
            return requests.Select(a => new Answer
            {
                Id = Utils.Utils.GenerateIdModel("Answer"),
                QuestionId = questionId,
                Text = a.Text,
                Score = a.Score,
                Tag = a.Tag,
                CreatedAt = Utils.Utils.GetTimeNow(),
                Status = 1
            }).ToList();
        }
    }
}
