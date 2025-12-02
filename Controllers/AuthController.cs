using Microsoft.AspNetCore.Mvc;
using SocietyManagementAPI.DTO;
using SocietyManagementAPI.Service;

namespace SocietyManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly CommonService _commonService;

        public AuthController(AuthService authService, CommonService commonService)
        {
            _authService = authService;
            _commonService = commonService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            try
            {
                var response = await _authService.LoginAsync(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
