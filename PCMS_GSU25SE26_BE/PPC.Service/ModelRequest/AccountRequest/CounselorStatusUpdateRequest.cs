using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.AccountRequest
{
    public class CounselorStatusUpdateRequest
    {
        public string CounselorId { get; set; }
        public int Status { get; set; }
    }
}
