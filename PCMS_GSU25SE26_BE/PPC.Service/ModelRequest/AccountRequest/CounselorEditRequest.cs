using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.AccountRequest
{
    public class CounselorEditRequest
    {
        public string Fullname { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
        public string Phone { get; set; }
        public int? YearOfJob { get; set; }
        public string Avatar { get; set; }
    }
}
