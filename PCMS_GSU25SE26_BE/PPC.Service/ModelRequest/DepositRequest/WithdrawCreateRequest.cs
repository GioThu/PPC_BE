using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.DepositRequest
{
    public class WithdrawCreateRequest
    {
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Total must be greater than 0.")]
        public double Total { get; set; }

        [Required]
        [MaxLength(50)]
        public string Stk { get; set; }

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountName { get; set; }
    }
}
