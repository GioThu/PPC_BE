using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.SurveyRequest
{
    public class SurveyResultRequest
    {
        public string SurveyId { get; set; }
        public List<TagScoreItem> Answers { get; set; }
    }

    public class TagScoreItem
    {
        public string Tag { get; set; }
        public int Score { get; set; }
    }
}
