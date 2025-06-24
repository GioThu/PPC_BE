using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.SurveyRequest
{
    public class SurveyQuestionCreateRequest
    {
        public string SurveyId { get; set; }
        public string Description { get; set; }
        public List<SurveyAnswerCreateRequest> Answers { get; set; }
    }
}
