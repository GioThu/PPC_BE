using PPC.Service.ModelResponse.MemberResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.Couple
{
    public class CoupleRoomResponse
    {
        public string Id { get; set; }
        public bool IsOwner { get; set; }
        public MemberDto Member { get; set; }
        public MemberDto Member1 { get; set; }
        public string AccessCode { get; set; }
        public bool? IsVirtual { get; set; }
        public string VirtualName { get; set; }
        public string? VirtualAvatar { get; set; }
        public string? VirtualDescription { get; set; }
        public string? VirtualRelationship { get; set; }
        public string? VirtualGender { get; set; }
        public DateTime? CreateAt { get; set; }
        public int? Status { get; set; }
    }
}
