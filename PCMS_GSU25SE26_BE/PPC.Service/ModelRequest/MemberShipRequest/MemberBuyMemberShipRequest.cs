using System.ComponentModel.DataAnnotations;

namespace PPC.Service.ModelRequest.MemberShipRequest
{
    public class MemberBuyMemberShipRequest
    {
        [Required]
        public string MemberShipId { get; set; }
    }
}