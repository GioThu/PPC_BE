using PPC.Service.ModelResponse.CoupleResponse;
using PPC.Service.ModelResponse.PersonTypeResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.BookingResponse
{
    public class MemberPersonTypeBlockDto
    {
        public List<ResultHistoryResponse> Mbti { get; set; } = new();
        public List<ResultHistoryResponse> Disc { get; set; } = new();
        public List<ResultHistoryResponse> LoveLanguage { get; set; } = new();
        public List<ResultHistoryResponse> BigFive { get; set; } = new();
    }

    public class PersonTypeBundleDto
    {
        public MemberPersonTypeBlockDto Member1 { get; set; }
        public MemberPersonTypeBlockDto Member2 { get; set; }
        public CoupleResultDto Couple { get; set; }
    }

    public class GetPersonTypeBundleRequest
    {
        public string BookingId { get; set; }
    }
}
