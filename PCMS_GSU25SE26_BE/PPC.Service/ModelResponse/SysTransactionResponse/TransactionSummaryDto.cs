using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.SysTransactionResponse
{
    public class TransactionSummaryDto
    {
        public string Id { get; set; }
        public string TransactionType { get; set; }
        public string DocNo { get; set; }
        public Double Amount { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Description { get; set; } 
    }
}
