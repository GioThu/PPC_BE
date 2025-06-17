using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.MemberShipResponse
{
    public class MyMemberShipStatusResponse
    {
        public MemberShipDto? MemberShip { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsActive { get; set; }
    }
}
