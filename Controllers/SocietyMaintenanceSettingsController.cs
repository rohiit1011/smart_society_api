using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.Model;
using SocietyManagementAPI.Service;

namespace SocietyManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocietyMaintenanceSettingsController : ControllerBase
    {
        private readonly SocietyContext _context;
        private readonly CommonService _commonService;

        public SocietyMaintenanceSettingsController(SocietyContext context, CommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        [HttpPost("AddOrUpdate")]
        public async Task<IActionResult> AddOrUpdate([FromBody] SocietyMaintenanceSettings model)
        {
            try
            {
                if (model == null || model.society_id <= 0)
                {
                    return BadRequest(await _commonService.generateResponse(false, null, "Invalid request data."));
                }

                var societyExists = await _context.societies
                    .AnyAsync(s => s.society_id == model.society_id);

                if (!societyExists)
                {
                    return NotFound(await _commonService.generateResponse(false, null, "Society not found."));
                }

                // 🔍 Check if maintenance setting already exists
                var existingSetting = await _context.societyMaintenanceSettings
                    .FirstOrDefaultAsync(s => s.society_id == model.society_id);

                if (existingSetting == null)
                {
                    model.created_at = ConvertToUtc(DateTime.UtcNow);
                    model.updated_at = ConvertToUtc(DateTime.UtcNow);
                    await _context.societyMaintenanceSettings.AddAsync(model);
                    await _context.SaveChangesAsync();

                    return Ok(await _commonService.generateResponse(true, model, "Maintenance settings added successfully."));
                }
                else
                {
                    existingSetting.maintenance_type = model.maintenance_type;
                    existingSetting.amount = model.amount;
                    existingSetting.due_day = model.due_day;
                    existingSetting.penalty_after_days = model.penalty_after_days;
                    existingSetting.penalty_amount = model.penalty_amount;
                    existingSetting.maintenance_frequency = model.maintenance_frequency ?? "Monthly";
                    existingSetting.is_active = model.is_active;
                    existingSetting.updated_by = model.updated_by;
                    existingSetting.updated_at = ConvertToUtc(DateTime.UtcNow);

                    _context.societyMaintenanceSettings.Update(existingSetting);
                    await _context.SaveChangesAsync();

                    return Ok(await _commonService.generateResponse(true, existingSetting, "Maintenance settings updated successfully."));
                }
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
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
