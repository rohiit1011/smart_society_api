using Microsoft.AspNetCore.Mvc;
using SocietyManagementAPI.DTO;
using SocietyManagementAPI.Interface;
using SocietyManagementAPI.Service;

namespace SocietyManagementAPI.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SocietyManagementAPI.Model;

    [ApiController]
    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _svc;
        private readonly ICommonService _commonService;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(IMaintenanceService svc, ICommonService commonService, ILogger<MaintenanceController> logger)
        {
            _svc = svc;
            _commonService = commonService;
            _logger = logger;
        }

        [HttpPost("maintenance-preview")]
        public async Task<IActionResult> Preview([FromBody] MaintenanceGenerateRequest req)
        {
            try
            {
                var data = await _svc.PreviewGenerateAsync(req);
                return Ok(await _commonService.generateResponse(true, data, "Preview ready"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Preview error");
                return StatusCode(500, await _commonService.generateResponse(false, null, ex.Message));
            }
        }

        [HttpPost("generate-maintenance")]
        public async Task<IActionResult> Generate([FromBody] MaintenanceGenerateRequest req)
        {
            try
            {
                // replace this with actual current user id extraction
                var currentUserId = int.TryParse(User?.FindFirst("user_id")?.Value, out var id) ? id : 0;
                var result = await _svc.GenerateRunAsync(req, currentUserId);
                return Ok(await _commonService.generateResponse(true, result, "Bills generated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generate error");
                return StatusCode(500, await _commonService.generateResponse(false, null, ex.Message));
            }
        }

        [HttpGet("fetch-bill-by-society")]
        public async Task<IActionResult> Runs([FromQuery] int societyId)
        {
            try
            {
                var data = await _svc.FetchBillRunsAsync(societyId);
                return Ok(await _commonService.generateResponse(true, data, "Runs fetched"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Runs error");
                return StatusCode(500, await _commonService.generateResponse(false, null, ex.Message));
            }
        }

        [HttpGet("run/{runId}/bills")]
        public async Task<IActionResult> BillsForRun([FromRoute] int runId)
        {
            try
            {
                var data = await _svc.FetchBillsForRunAsync(runId);
                return Ok(await _commonService.generateResponse(true, data, "Bills fetched"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BillsForRun error");
                return StatusCode(500, await _commonService.generateResponse(false, null, ex.Message));
            }
        }

        [HttpPost("bill/{billId}/send-notice")]
        public async Task<IActionResult> SendBillNotice([FromRoute] int billId)
        {
            try
            {
                var ok = await _svc.SendBillNoticeAsync(billId);
                return Ok(await _commonService.generateResponse(ok, ok, ok ? "Notice sent" : "Failed to send notice"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendBillNotice error");
                return StatusCode(500, await _commonService.generateResponse(false, null, ex.Message));
            }
        }

        // Add payment endpoint (basic)
        //[HttpPost("pay")]
        //public async Task<IActionResult> Pay([FromBody] PaymentRequest req)
        //{
        //    try
        //    {
        //        // IMPLEMENT simple payment record creation and bill status updates
        //        // ...
        //        return Ok(await _commonService.generateResponse(true, null, "Payment recorded"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Pay error");
        //        return StatusCode(500, await _commonService.generateResponse(false, null, ex.Message));
        //    }
        //}


        [HttpGet("get-all-maintenance-heads")]
        public async Task<IActionResult> GetAllHeads([FromQuery] int societyId)
        {
            try
            {
                if (societyId <= 0)
                    return BadRequest(await _commonService.generateResponse(false, null, "Invalid societyId"));

                var heads = await _svc.FetchAllHeadsAsync(societyId);

                return Ok(await _commonService.generateResponse(true, heads, "Maintenance heads fetched"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching maintenance heads");

                return StatusCode(500,
                    await _commonService.generateResponse(false, null, "Internal server error while fetching maintenance heads"));
            }
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddHeadAsync([FromBody] MaintenanceHeads dto)
        {
            try
            {
                var result = await _svc.AddHeadAsync(dto);
                return Ok(await _commonService.generateResponse(true, result, "Maintenance head added"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding maintenance head");

                return StatusCode(500,
                    await _commonService.generateResponse(false, null, "Error while adding maintenance head"));
            }
        }




    }

}
