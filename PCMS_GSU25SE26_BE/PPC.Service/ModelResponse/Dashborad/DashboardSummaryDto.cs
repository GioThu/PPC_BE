using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.Dashborad
{
    public class DashboardSummaryDto
    {
        public double CurrentBalance { get; set; }
        public double ThisMonthIncome { get; set; }
        public double PendingPayment { get; set; }
        public double WithdrawnTotal { get; set; }
        public double PendingDeposit { get; set; }
    }

    public class DashboardSummary
    {
        public double CurrentBalance { get; set; }
        public double ThisMonthIncome { get; set; }
        public double PendingPayment { get; set; }
        public double WithdrawnTotal { get; set; }
        public double PendingDeposit { get; set; }
    }
}
