using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.AccountRequest
{
    public class MemberStatusUpdateRequest
    {
        public string MemberId { get; set; }
        public int Status { get; set; } 
    }
}
