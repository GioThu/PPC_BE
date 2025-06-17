using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.TransactionRequest
{
    public class SysTransactionCreateRequest
    {
        public string TransactionType { get; set; }
        public string DocNo { get; set; }
        public string CreateBy { get; set; }
    }
}
