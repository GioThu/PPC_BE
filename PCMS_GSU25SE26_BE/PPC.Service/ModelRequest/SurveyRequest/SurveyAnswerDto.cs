using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.SurveyRequest
{
    public class SurveyAnswerDto
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int? Score { get; set; }
        public string Tag { get; set; }
    }
}
