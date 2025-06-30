using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.PersonTypeResponse
{
    public class ResultHistoryResponse
    {
        public string SurveyId { get; set; }
        public string Result { get; set; }
        public string Description { get; set; }
        public string RawScores { get; set; }
        public Dictionary<string, int> Scores { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
