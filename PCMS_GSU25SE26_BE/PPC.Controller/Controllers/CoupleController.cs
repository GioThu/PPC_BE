using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.Couple;
using PPC.Service.ModelRequest.SurveyRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CoupleResponse;

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

        [Authorize(Roles = "3")]
        [HttpPost("create-virtual")]
        public async Task<IActionResult> CreateVirtual([FromBody] VirtualCoupleCreateRequest request)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Member ID not found.");

            var result = await _coupleService.CreateVirtualCoupleAsync(memberId, request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "3")]
        [HttpPost("submit-virtual")]
        public async Task<IActionResult> SubmitVirtual([FromBody] SurveyResultRequest request)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Member ID not found.");

            var response = await _coupleService.SubmitVirtualResultAsync(memberId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")]
        [HttpPost("apply-latest-result")]
        public async Task<IActionResult> ApplyLatestResult([FromBody] ApplyResultRequest request)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Member ID not found.");

            var response = await _coupleService.ApplyLatestResultToSelfAsync(memberId, request.SurveyId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("by-booking")]
     
        public async Task<IActionResult> GetByBooking([FromBody] GetCoupleByBookingRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.BookingId))
                return BadRequest(ServiceResponse<CoupleResultDto>.ErrorResponse("Thiếu bookingId"));

            var res = await _coupleService.GetCoupleResultByBookingIdAsync(req.BookingId);
            if (res.Success)
                return Ok(res);
            return BadRequest(res);
        }

        [HttpGet("{coupleId}/subcategories")]
        public async Task<IActionResult> GetSubCategoriesByCoupleId([FromRoute] string coupleId)
        {
            var response = await _coupleService.GetSubCategoryNamesByCoupleIdAsync(coupleId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}

