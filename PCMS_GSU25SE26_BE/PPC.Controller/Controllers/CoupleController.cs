using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.Couple;
using PPC.Service.ModelRequest.SurveyRequest;

namespace PPC.Controller.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CoupleController : ControllerBase
    {
        private readonly ICoupleService _coupleService;

        public CoupleController(ICoupleService coupleService)
        {
            _coupleService = coupleService;
        }


        [HttpPost("join")]
        public async Task<IActionResult> JoinCouple([FromBody] JoinCoupleRequest request)
        {
            if (string.IsNullOrEmpty(request.AccessCode))
                return BadRequest("AccessCode is required.");

            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.JoinCoupleByAccessCodeAsync(memberId, request.AccessCode);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCouple([FromBody] CoupleCreateRequest request)
        {
            if (request.SurveyIds == null || !request.SurveyIds.Any())
                return BadRequest("SurveyIds is required.");

            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.CreateCoupleAsync(memberId, request);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPut("cancel-room")]
        public async Task<IActionResult> CancelLatestCouple()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.CancelLatestCoupleAsync(memberId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("get-latest-room")]
        public async Task<IActionResult> GetLatestCoupleDetail()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.GetLatestCoupleDetailAsync(memberId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("latest-status")]
        public async Task<IActionResult> GetLatestCoupleStatus()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.GetLatestCoupleStatusAsync(memberId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitSurveyResult([FromBody] SurveyResultRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.SubmitResultAsync(memberId, request);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("partner-progress")]
        public async Task<IActionResult> CheckPartnerSurveyProgress()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.CheckPartnerAllSurveysStatusAsync(memberId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("my-couples-history")]
        public async Task<IActionResult> GetMyCouples()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;

            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("memberId not found in token.");

            var response = await _coupleService.GetCouplesByMemberIdAsync(memberId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("result/{coupleId}")]
        public async Task<IActionResult> GetCoupleResultById(string coupleId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var result = await _coupleService.GetCoupleResultByIdAsync(coupleId, memberId);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteCouple(string id)
        {
            var response = await _coupleService.MarkCoupleAsCompletedAsync(id);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
