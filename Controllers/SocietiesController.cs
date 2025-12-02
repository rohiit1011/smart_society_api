using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.DTO;
using SocietyManagementAPI.Model;
using SocietyManagementAPI.Service;
using System.Net.Sockets;

namespace SocietyManagementAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SocietiesController : ControllerBase
    {
        private readonly SocietyContext _context;
        private readonly CommonService _commonService;


        public SocietiesController(SocietyContext context, CommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        [HttpGet("GetAllSocieties")]
        public async Task<IActionResult> GetSocieties()
        {
            try
            {
                var result = await _context.societies.ToListAsync();
               // throw new Exception("Dummy test exception for error handling check");

                if (result != null && result.Count > 0)
                {
                    var response = await _commonService.generateResponse(true, result, "Data Found");
                    return Ok(response);
                }
                else
                {
                    var response = await _commonService.generateResponse(false, result, "Data Not Found");
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                // _logger.LogError(ex, "Error fetching societies.");

                var errorResponse = await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("GetSocietyWings")]
        public async Task<IActionResult> GetSocietyWings([FromQuery] int societyId)
        {
            try
            {
                var result = await _context.societyWings.Where((filter) => filter.society_id==societyId).ToListAsync();
                // throw new Exception("Dummy test exception for error handling check");

                if (result != null && result.Count > 0)
                {
                    var response = await _commonService.generateResponse(true, result, "Data Found");
                    return Ok(response);
                }
                else
                {
                    var response = await _commonService.generateResponse(false, result, "Data Not Found");
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                // _logger.LogError(ex, "Error fetching societies.");

                var errorResponse = await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterSociety([FromBody] Societies societyDetails)
        {
            try
            {

                var existingSociety = await _context.societies
                    .FirstOrDefaultAsync(s => s.email == societyDetails.email);

                if (existingSociety != null)
                {
                    // Society with this email already exists
                    var errorResponse = await _commonService.generateResponse(
                        false,
                        null,
                        $"Society with email {societyDetails.email} is already registered."
                    );
                    return Ok(errorResponse); // HTTP 409 Conflict
                }

                var nowUtc = DateTime.UtcNow;

                //societyDetails.registration_date = nowUtc;
               societyDetails.created_at = nowUtc;
                societyDetails.updated_at = nowUtc;
                societyDetails.society_code = GenerateSocietyCode();
                var society = societyDetails;

                _context.societies.Add(society);
                await _context.SaveChangesAsync();

                // Return society code
                var response = await _commonService.generateResponse(true, society, "Society registered successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        private string GenerateSocietyCode()
        {
            // Simple example: GV + timestamp
            return "GV" + DateTime.UtcNow.Ticks.ToString().Substring(10);
        }

        [HttpGet("GetSocietyAndAdminDetails")]
        public async Task<IActionResult> GetSocietyAndAdminDetails([FromQuery] int societyId)
        {
            try
            {
                var societyData = await _context.societies
                    .Where(s => s.society_id == societyId)
                    .Select(s => new
                    {
                        s.society_id,
                        s.society_code,
                        s.name,
                        s.email,
                        s.phone,
                        s.city,
                        s.pincode,
                        s.address,
                        s.is_active,
                        s.created_at,
                        s.updated_at,
                        s.created_by,
                        s.updated_by,
                        s.verification_status,

                        // ✅ Get Society Details if present
                        SocietyDetails = _context.societyDetails
                            .Where(sd => sd.society_id == s.society_id)
                            .Select(sd => new
                            {
                                sd.society_detail_id,
                                sd.water_connection_type,
                                sd.establishment_year,
                                sd.pan_number,
                                sd.gst_number,
                                sd.registration_no,
                                sd.registration_date,
                                sd.electricity_provider,
                                sd.has_generator,
                                sd.has_lift,
                                sd.updated_by,
                                sd.updated_at
                            })
                            .FirstOrDefault(),

                        // ✅ Get Admin details (role_id = 2)
                        AdminDetails = _context.userRoles
                            .Where(ur => ur.society_id == s.society_id && ur.role_id == 2)
                            .Join(_context.users,
                                  ur => ur.user_id,
                                  u => u.user_id,
                                  (ur, u) => new
                                  {
                                      u.user_id,
                                      u.first_name,
                                      u.middle_name,
                                      u.last_name,
                                      u.email,
                                      u.phone,
                                      u.is_active,
                                      ur.assigned_at
                                  })
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (societyData == null)
                {
                    var notFoundResponse = await _commonService.generateResponse(false, null, "Society not found.");
                    return NotFound(notFoundResponse);
                }

                var response = await _commonService.generateResponse(true, societyData, "Society and admin details fetched successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }



        [HttpGet("GetSocietyFullDetails")]
        public async Task<IActionResult> GetSocietyFullDetails([FromQuery] int societyId)
        {
            try
            {
                var society = await _context.societies
                    .Where(s => s.society_id == societyId)
                    .Select(s => new
                    {
                        s.society_id,
                        s.society_code,
                        s.name,
                        s.email,
                        s.phone,
                        s.city,
                        s.pincode,
                        s.address,
                        s.is_active,
                        created_at = s.created_at.HasValue ? s.created_at.Value.ToUniversalTime() : (DateTime?)null,
                        updated_at = s.updated_at.HasValue ? s.updated_at.Value.ToUniversalTime() : (DateTime?)null,
                        s.created_by,
                        s.updated_by,
                        s.verification_status,

                        // 🔹 Admin Details
                        adminDetails = _context.userRoles
                                .Where(ur => ur.society_id == s.society_id && ur.role_id == 2)
                                .Join(_context.users,
                                      ur => ur.user_id,
                                      u => u.user_id,
                                      (ur, u) => new
                                      {
                                          u.user_id,
                                          u.first_name,
                                          u.last_name,
                                          u.middle_name,
                                          u.email,
                                          u.phone,
                                          u.is_active,
                                          assigned_at = ur.assigned_at
                                      })
                                .FirstOrDefault(),

                        // 🔹 Extended Society Details
                        societyDetails = _context.societyDetails
                                .Where(sd => sd.society_id == s.society_id)
                                .Select(sd => new
                                {
                                    sd.establishment_year,
                                    sd.pan_number,
                                    sd.gst_number,
                                    sd.water_connection_type,
                                    sd.electricity_provider,
                                    sd.has_lift,
                                    sd.has_generator
                                })
                                .FirstOrDefault() ?? new { establishment_year = 0, pan_number = "", gst_number = "", water_connection_type = "", electricity_provider = "", has_lift = false, has_generator = false },

                        // 🔹 Wings
                        societyWings = _context.societyWings
                                .Where(sw => sw.society_id == s.society_id)
                                .Select(sw => new
                                {
                                    sw.wing_id,
                                    sw.wing_name,
                                    sw.number_of_floors,
                                    sw.total_flats,
                                    sw.occupied_flats,
                                    sw.society_id,
                                    //sw.created_at,
                                    //sw.updated_at,
                                    sw.created_by,
                                    sw.updated_by
                                }).ToList(),

                        // 🔹 Maintenance Settings
                        Maintenance = _context.societyMaintenanceSettings
                                .Where(ms => ms.society_id == s.society_id)
                                .Select(ms => new
                                {
                                    ms.setting_id,
                                    ms.society_id,
                                    ms.maintenance_type,
                                    ms.maintenance_frequency,
                                    ms.amount,
                                    ms.due_day,
                                    ms.penalty_after_days,
                                    ms.penalty_amount,
                                    ms.is_active,
                                    ms.created_by,
                                    ms.created_at,
                                    ms.updated_by,
                                    ms.updated_at
                                })
                                .FirstOrDefault(),    
                    })
                    .FirstOrDefaultAsync();

                if (society == null)
                {
                    var notFoundResponse = await _commonService.generateResponse(false, null, "Society not found.");
                    return NotFound(notFoundResponse);
                }

                var response = await _commonService.generateResponse(true, society, "Society details fetched successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, $"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }




        [HttpGet("GetWingsAndMaintenanceDetails")]
        public async Task<IActionResult> GetWingsAndMaintenanceDetails([FromQuery] int societyId)
        {
            try
            {
                var society = await _context.societies
                  .Where(s => s.society_id == societyId)
                  .Select(s => new
                  {
                      // 🔹 Wings
                      Wings = _context.societyWings
                                .Where(sw => sw.society_id == s.society_id)
                                .Select(sw => new
                                {
                                    sw.wing_id,
                                    sw.wing_name,
                                    sw.number_of_floors,
                                    sw.total_flats,
                                    sw.occupied_flats,
                                    sw.society_id,
                                    //sw.created_at,
                                    //sw.updated_at,
                                    sw.created_by,
                                    sw.updated_by
                                }).ToList(),

                        // 🔹 Maintenance Settings
                        Maintenance = _context.societyMaintenanceSettings
                                .Where(ms => ms.society_id == s.society_id)
                                .Select(ms => new
                                {
                                    ms.setting_id,
                                    ms.society_id,
                                    ms.maintenance_type,
                                    ms.maintenance_frequency,
                                    ms.amount,
                                    ms.due_day,
                                    ms.penalty_after_days,
                                    ms.penalty_amount,
                                    ms.is_active,
                                    ms.created_by,
                                    ms.created_at,
                                    ms.updated_by,
                                    ms.updated_at
                                })
                                .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (society == null)
                {
                    var notFoundResponse = await _commonService.generateResponse(false, null, "Society not found.");
                    return NotFound(notFoundResponse);
                }

                var response = await _commonService.generateResponse(true, society, "Society details fetched successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, $"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPost("update-society-details")]
        public async Task<IActionResult> UpdateSocietyDetails([FromBody] SocietyDetails model)
        {
            try
            {
                var nowUtc = DateTime.UtcNow;

                // First, check if society exists
                var societyExists = await _context.societies
                    .AnyAsync(s => s.society_id == model.society_id);

                if (!societyExists)
                {
                    return NotFound(await _commonService.generateResponse(
                        false, null, "Society not found."));
                }

                // -----------------------------
                // Update main society table
                // -----------------------------
                string updateSocietySql = @"
            UPDATE master.society
            SET name = @name,
                address = @address,
                phone = @phone,
                city = @city,
                pincode = @pincode
            WHERE society_id = @society_id;
        ";

                await _context.Database.ExecuteSqlRawAsync(updateSocietySql,
                    new Npgsql.NpgsqlParameter("@name", model.societyRegisterDto.name),
                    new Npgsql.NpgsqlParameter("@address", model.societyRegisterDto.address ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@phone", model.societyRegisterDto.phone ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@city", model.societyRegisterDto.city),
                    new Npgsql.NpgsqlParameter("@pincode", model.societyRegisterDto.pincode),
                    new Npgsql.NpgsqlParameter("@society_id", model.society_id)
                );

                // -----------------------------
                // Insert or update society_details table
                // -----------------------------
                string insertUpdateDetailsSql = @"
            INSERT INTO master.society_details
                (society_id, water_connection_type, establishment_year, pan_number, gst_number,
                 registration_no, registration_date, electricity_provider, has_generator, has_lift,
                 updated_by, updated_at)
            VALUES
                (@society_id, @water_connection_type, @establishment_year, @pan_number, @gst_number,
                 @registration_no, @registration_date, @electricity_provider, @has_generator, @has_lift,
                 @updated_by, @updated_at)
            ON CONFLICT (society_id) 
            DO UPDATE SET
                water_connection_type = EXCLUDED.water_connection_type,
                establishment_year = EXCLUDED.establishment_year,
                pan_number = EXCLUDED.pan_number,
                gst_number = EXCLUDED.gst_number,
                registration_no = EXCLUDED.registration_no,
                registration_date = EXCLUDED.registration_date,
                electricity_provider = EXCLUDED.electricity_provider,
                has_generator = EXCLUDED.has_generator,
                has_lift = EXCLUDED.has_lift,
                updated_by = EXCLUDED.updated_by,
                updated_at = EXCLUDED.updated_at;
        ";

                await _context.Database.ExecuteSqlRawAsync(insertUpdateDetailsSql,
                    new Npgsql.NpgsqlParameter("@society_id", model.society_id),
                    new Npgsql.NpgsqlParameter("@water_connection_type", model.water_connection_type ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@establishment_year", model.establishment_year),
                    new Npgsql.NpgsqlParameter("@pan_number", model.pan_number ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@gst_number", model.gst_number ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@registration_no", model.registration_no ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@registration_date", model.registration_date!=null
                        ? (object)model.registration_date.ToUniversalTime() : DBNull.Value),
                    new Npgsql.NpgsqlParameter("@electricity_provider", model.electricity_provider ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@has_generator", model.has_generator),
                    new Npgsql.NpgsqlParameter("@has_lift", model.has_lift),
                    new Npgsql.NpgsqlParameter("@updated_by", model.updated_by ?? (object)DBNull.Value),
                    new Npgsql.NpgsqlParameter("@updated_at", nowUtc)
                );

                return Ok(await _commonService.generateResponse(
                    true, new { model.society_detail_id }, "Society details updated successfully."));
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
