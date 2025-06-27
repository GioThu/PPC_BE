using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.DepositRequest
{
    public class DepositCreateRequest
    {
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Total must be greater than 0.")]
        public double Total { get; set; }

    }
}
