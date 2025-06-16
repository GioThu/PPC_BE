using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CirtificationResponse
{
    public class ApproveCertificationRequest
    {
        [Required]
        public string CertificationId { get; set; }
    }
}
