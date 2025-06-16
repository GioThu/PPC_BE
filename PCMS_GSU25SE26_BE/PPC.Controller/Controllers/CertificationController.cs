using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.CirtificationRequest;

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
    }
}
