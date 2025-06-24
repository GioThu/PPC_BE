using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.SurveyResponse
{
    public class SurveyQuestionDto
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<SurveyAnswerDto> Answers { get; set; }
    }
}
