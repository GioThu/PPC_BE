using PPC.Service.ModelRequest.SurveyRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CourseRequest
{
    public class QuestionCreateRequest
    {
        public string QuizId { get; set; }
        public string Description { get; set; }
        public List<AnswerCreateRequest> Answers { get; set; }
    }
}
