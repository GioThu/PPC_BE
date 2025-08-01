using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.Couple
{
    public class VirtualCoupleCreateRequest
    {
        public List<string> SurveyIds { get; set; }  // Ví dụ: ["SV001", "SV002"]
        public string VirtualName { get; set; }
        public string? VirtualAvatar { get; set; }
        public string? VirtualDescription { get; set; }
        public string? VirtualRelationship { get; set; }
        public string? VirtualGender { get; set; }
        public DateTime? VirtualDob { get; set; }
    }
}
