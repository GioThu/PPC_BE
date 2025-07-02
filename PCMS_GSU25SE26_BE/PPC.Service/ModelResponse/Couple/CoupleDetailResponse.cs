using PPC.Service.ModelResponse.MemberResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.Couple
{
    public class CoupleDetailResponse
    {
        public string Id { get; set; }
        public MemberDto Member { get; set; }
        public MemberDto Member1 { get; set; }
        public string Mbti { get; set; }
        public string Disc { get; set; }
        public string LoveLanguage { get; set; }
        public string BigFive { get; set; }
        public string Mbti1 { get; set; }
        public string Disc1 { get; set; }
        public string LoveLanguage1 { get; set; }
        public string BigFive1 { get; set; }
        public string MbtiResult { get; set; }
        public string DiscResult { get; set; }
        public string LoveLanguageResult { get; set; }
        public string BigFiveResult { get; set; }
        public bool? IsVirtual { get; set; }
        public string VirtualName { get; set; }
        public DateTime? VirtualDob { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Rec1 { get; set; }
        public string Rec2 { get; set; }
        public int? Status { get; set; }
        public string AccessCode { get; set; }
    }
}
