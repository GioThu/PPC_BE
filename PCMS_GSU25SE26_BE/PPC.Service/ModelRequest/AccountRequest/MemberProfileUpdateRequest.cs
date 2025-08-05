using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.AccountRequest
{
    public class MemberProfileUpdateRequest
    {
        public string? Fullname { get; set; }
        public string? Avatar { get; set; }
        public string? Phone { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
    }
}
