using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.DepositRequest
{
    public class DepositChangeStatusRequest
    {
        [Required]
        public string DepositId { get; set; }

        [Required]
        public int NewStatus { get; set; } 

        public string? CancelReason { get; set; }
    }
}
