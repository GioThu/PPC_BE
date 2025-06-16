using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CirtificationRequest
{
    public class RejectCertificationRequest
    {
        [Required]
        public string CertificationId { get; set; }

        [Required]
        [MaxLength(500)]
        public string RejectReason { get; set; }
    }
}
