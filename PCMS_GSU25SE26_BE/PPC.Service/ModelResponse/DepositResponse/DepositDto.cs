using PPC.Service.ModelResponse.CounselorResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.DepositResponse
{
    public class DepositDto
    {
        public string Id { get; set; }
        public string WalletId { get; set; }
        public double? Total { get; set; }
        public string Stk { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CancelReason { get; set; }
        public int? Status { get; set; }

        public CounselorDto Counselor { get; set; }
    }
}
