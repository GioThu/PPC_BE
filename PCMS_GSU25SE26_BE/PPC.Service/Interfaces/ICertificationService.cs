using PPC.Service.ModelRequest.CirtificationRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CirtificationResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ICertificationService
    {
        Task<ServiceResponse<string>> SendCertificationAsync(string counselorId, SendCertificationRequest request);
        Task<ServiceResponse<string>> ApproveCertificationAsync(string certificationId);
        Task<ServiceResponse<string>> RejectCertificationAsync(RejectCertificationRequest request);
        Task<ServiceResponse<List<CertificationWithSubDto>>> GetMyCertificationsAsync(string counselorId);
        Task<ServiceResponse<List<CertificationWithSubDto>>> GetAllCertificationsAsync();
        Task<ServiceResponse<string>> UpdateCertificationAsync(string counselorId, UpdateCertificationRequest request);

    }
}
