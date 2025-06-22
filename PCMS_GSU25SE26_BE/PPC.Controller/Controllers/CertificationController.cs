using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.CirtificationRequest;
using PPC.Service.Services;
using System.Security.Claims;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificationController : ControllerBase
    {
        private readonly ICertificationService _certificationService;

        public CertificationController(ICertificationService certificationService)
        {
            _certificationService = certificationService;
        }

        [Authorize(Roles = "2")]
        [HttpPost("send")]
        public async Task<IActionResult> SendCertification([FromBody] SendCertificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("Counselor not found.");

            var response = await _certificationService.SendCertificationAsync(counselorId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }


        [Authorize(Roles = "1")] 
        [HttpPut("approve/{certificationId}")]
        public async Task<IActionResult> ApproveCertification(string certificationId)
        {
            var response = await _certificationService.ApproveCertificationAsync(certificationId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")] 
        [HttpPut("reject")]
        public async Task<IActionResult> RejectCertification([FromBody] RejectCertificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _certificationService.RejectCertificationAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2")]
        [HttpGet("my-certifications")]
        public async Task<IActionResult> GetMyCertifications()
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("Counselor not found.");

            var response = await _certificationService.GetMyCertificationsAsync(counselorId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")] 
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCertifications()
        {
            var response = await _certificationService.GetAllCertificationsAsync();
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCertification([FromBody] UpdateCertificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("Counselor not found.");

            var response = await _certificationService.UpdateCertificationAsync(counselorId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
        [HttpGet("all-paging")]
        public async Task<IActionResult> GetAllCertifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? status = null)
        {
            var response = await _certificationService.GetAllCertificationsAsync(pageNumber, pageSize, status);

            if (response.Success)
            {
                return Ok(response.Data);  
            }
            return BadRequest(response);
        }

        [Authorize(Roles = "1,2")]
        [HttpGet("Get-detail")]
        public async Task<IActionResult> GetCertificationById(string certificationId)
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if  (role == "2")
            {
                if (await _certificationService.IsCertificationAssignedToCounselorAsync(certificationId, User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value) == false)
                {
                    return Unauthorized("You do not have permission to view this certification.");
                }
            }
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            var response = await _certificationService.GetCertificationByIdAsync(certificationId);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
