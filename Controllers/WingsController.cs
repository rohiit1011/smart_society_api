using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.Model;
using SocietyManagementAPI.Service;

namespace SocietyManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WingsController : ControllerBase
    {
        private readonly SocietyContext _context;
        private readonly CommonService _commonService;

        public WingsController(SocietyContext context, CommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        [HttpPost("AddWing")]
        public async Task<IActionResult> AddWing([FromBody] SocietyWings model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.wing_name))
                {
                    return BadRequest(await _commonService.generateResponse(false, null, "Wing name and society ID are required."));
                }

                var societyExists = await _context.societies.AnyAsync(s => s.society_id == model.society_id);
                if (!societyExists)
                {
                    return NotFound(await _commonService.generateResponse(false, null, "Society not found."));
                }

                // Check if wing already exists in same society
                var existingWing = await _context.societyWings
                    .FirstOrDefaultAsync(w => w.society_id == model.society_id && w.wing_name == model.wing_name);

                if (existingWing != null)
                {
                    return Ok(await _commonService.generateResponse(false, null, "Wing already exists for this society."));
                }

                model.created_at = ConvertToUtc(DateTime.UtcNow);
                model.updated_at = ConvertToUtc(DateTime.UtcNow);

                await _context.societyWings.AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok(await _commonService.generateResponse(true, model, "Wing added successfully."));
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpDelete("DeleteWing/{wingId}")]
        public async Task<IActionResult> DeleteWing(int wingId)
        {
            try
            {
                // 🔍 Check if the wing exists
                var wing = await _context.societyWings
                    .FirstOrDefaultAsync(w => w.wing_id == wingId);

                if (wing == null)
                {
                    return NotFound(await _commonService.generateResponse(
                        false, null, "Wing not found."));
                }

                // ❌ Remove the wing
                _context.societyWings.Remove(wing);
                await _context.SaveChangesAsync();

                return Ok(await _commonService.generateResponse(
                    true, new { wingId }, "Wing deleted successfully."));
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(
                    false, null, $"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        public static DateTime ConvertToUtc(DateTime date)
        {
            if (date.Kind == DateTimeKind.Local)
                return date.ToUniversalTime();
            if (date.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();
            return date;
        }

    }
}
