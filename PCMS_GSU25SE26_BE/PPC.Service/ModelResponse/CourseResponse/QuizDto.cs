using PPC.Service.ModelResponse.SurveyResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class QuizDto
    {
        public string Id { get; set; }
        public int TotalScore { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }
}
