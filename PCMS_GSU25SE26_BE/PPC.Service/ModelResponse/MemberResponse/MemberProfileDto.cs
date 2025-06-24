using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.MemberResponse
{
    public class MemberProfileDto
    {
        public string Fullname { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; }
        public DateTime? Dob { get; set; }
        public string Mbti { get; set; }
        public string Disc { get; set; }
        public string LoveLanguage { get; set; }
        public string BigFive { get; set; }
        public string Gender { get; set; }
    }
}
