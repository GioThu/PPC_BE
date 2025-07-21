using PPC.Service.ModelRequest.CourseRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.SurveyRequest
{
    public class SurveyQuestionUpdateRequest
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<SurveyAnswerCreateRequest> Answers { get; set; }
    }

    public class QuestionUpdateRequest
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<AnswerCreateRequest> Answers { get; set; }
    }
}
