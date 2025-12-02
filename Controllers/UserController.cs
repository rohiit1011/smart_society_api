using HarfBuzzSharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.DTO;
using SocietyManagementAPI.Helpers;
using SocietyManagementAPI.Interface;
using SocietyManagementAPI.Model;
using SocietyManagementAPI.Service;
using System.Net.Sockets;

namespace SocietyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly SocietyContext _context;
        private readonly CommonService _commonService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public UserController(SocietyContext context, CommonService commonService, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _commonService = commonService;
            _emailService = emailService;
            _config = config;
        }

        // POST: api/user/register-for-society
        [HttpPost("register-for-society")]
        public async Task<IActionResult> RegisterAdminForSociety([FromBody] UserModel dto)
        {
            try
            {
                var nowUtc = DateTime.UtcNow;
                // 1️⃣ Check if society exists
                var society = await _context.societies.FirstOrDefaultAsync(s => s.society_code == dto.society_code && s.email==dto.society_email);
                if (society == null)
                {
                    var notFoundResponse = await _commonService.generateResponse(false, null, "Society not found.");
                    return NotFound(notFoundResponse);
                }
                // 2️⃣ Check if user (admin) already exists by email or username
                var existingUser = await _context.users
                    .FirstOrDefaultAsync(u => u.email == dto.email || u.username == dto.username);

                if (existingUser != null)
                {
                    var duplicateResponse = await _commonService.generateResponse(false, null,
                        "An account with this email or username already exists.");
                    return Ok(duplicateResponse);
                }

                // 2️⃣ Create Admin/User
                dto.updated_at = nowUtc;
                dto.created_at = nowUtc;
                
                var admin = dto;

                _context.users.Add(admin);
                await _context.SaveChangesAsync(); // Need this to get admin.Id

                // 3️⃣ Assign Role in user_roles
                // Assuming role_id for 'Admin' is known (e.g., 1)
                var adminRole = new UserRole
                {
                    user_id = admin.user_id,
                    society_id = society.society_id,
                    role_id = 2, // Admin Role ID
                    assigned_at = nowUtc
                };

                _context.userRoles.Add(adminRole);
                await _context.SaveChangesAsync();

                await SendSocietyRegistrationEmailAsync(admin, society);


                var response = await _commonService.generateResponse(true, null, "Admin registered successfully. Activation link sent to email.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
         
        [HttpPost("register-resident")]
        public async Task<IActionResult> RegisterResidentAsync([FromBody] ResidentInfo residentInfo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (residentInfo == null)
                    return Ok(await _commonService.generateResponse(false, null, "Invalid request data."));

                // 🔍 Check if user already exists
                var existingResident = await (
                     from u in _context.users
                     join ur in _context.userRoles on u.user_id equals ur.user_id
                     where (u.email == residentInfo.email || u.username == residentInfo.username)
                           && ur.role_id == RoleConstants.Owner
                     select u
                 ).FirstOrDefaultAsync();

                if (existingResident != null)
                    return Ok(await _commonService.generateResponse(false, null, "A user with this email or username already exists."));

                var now = DateTime.UtcNow;

                // 🧑‍💼 Create new user
                var user = new UserModel
                {
                    username = residentInfo.username,
                    email = residentInfo.email,
                    phone = residentInfo.phone,
                    password_hash = residentInfo.password_hash, // TODO: hash before save
                    first_name = residentInfo.first_name,
                    last_name= residentInfo.last_name,
                    middle_name = residentInfo.middle_name,
                    created_at = now,
                    updated_at = now,
                    is_active = false,
                    loginLink = residentInfo.loginLink,
                    //society_code = residentInfo.society_code,
                };

                _context.users.Add(user);
                await _context.SaveChangesAsync();

                // 🎭 Assign role
                var userRole = new UserRole
                {
                    user_id = user.user_id,
                    role_id = residentInfo.role_id,
                    society_id = residentInfo.society_id,
                    assigned_at = now
                };

                _context.userRoles.Add(userRole);
                await _context.SaveChangesAsync();

                // 🏠 Create resident record
                var resident = new ResidentInfo
                {
                    first_name = residentInfo.first_name,
                    last_name = residentInfo.last_name,
                    phone = residentInfo.phone,
                    email = residentInfo.email,
                    gender = residentInfo.gender,
                    user_id=user.user_id,
                    date_of_birth = residentInfo.date_of_birth,
                    aadhar_number = residentInfo.aadhar_number,
                    pan_number = residentInfo.pan_number,
                    photo_path = residentInfo.photo_path,
                    created_at = now,
                    updated_at = now,
                    verification_status="draft"

                    
                };

                _context.residentInfo.Add(resident);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var resultData = new
                {
                    user_id = user.user_id,
                    resident_id = resident.resident_id,
                    role_id = userRole.role_id
                };

                return Ok(await _commonService.generateResponse(true, resultData, "Resident registered successfully."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("send-resident-registration-email")]
        public async Task<IActionResult> SendResidentRegistrationEmail([FromQuery] int residentId)
        {
           bool result= await SendResidentRegistrationEmailAsync(residentId);
            if (result)
            {
                return Ok(await _commonService.generateResponse(
                    true,
                    null,
                    "Email sent successfully."
                ));
            }
            else
            {
                return Ok(await _commonService.generateResponse(
                    false,
                    null,
                    "Error while sending email."
                ));
            }

        }

        [HttpGet("get-all-roles")]
        public async Task<IActionResult> GetAllUserRoles()
        {
            var result = await _context.roles.ToListAsync();
            if (result!=null)
            {
                return Ok(await _commonService.generateResponse(
                    true,
                    result,
                    "Roles found."
                ));
            }
            else
            {
                return Ok(await _commonService.generateResponse(
                    false,
                    null,
                    "Error while fetching roles."
                ));
            }

        }


        [HttpPost("assign-roles-to-resident")]
        public async Task<IActionResult> AssignRoleToResident(UserRole userRole)
        {

            try
            {
                if (userRole == null)
                {
                    return BadRequest(await _commonService.generateResponse(
                        false,
                        null,
                        "Invalid user role data."
                    ));
                }


                // Step 2️⃣ Fetch required details
                var society = await _context.societies.FirstOrDefaultAsync(s => s.society_id == userRole.society_id);
                var user = await _context.users.FirstOrDefaultAsync(u => u.user_id == userRole.user_id);
                var role = await _context.roles.FirstOrDefaultAsync(r => r.role_id == userRole.role_id);

                if (society == null)
                    return Ok(await _commonService.generateResponse(false, null, "Society not found."));
                if (user == null)
                    return Ok(await _commonService.generateResponse(false, null, "User not found."));
                if (role == null)
                    return Ok(await _commonService.generateResponse(false, null, "Role not found."));


                userRole.assigned_at = DateTime.Now;

                var result = await _context.userRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                if (result.Entity != null)
                {

                   bool responde=await SendCommitteeRoleAssignmentEmailAsync(user, society, role.role_name);

                    return Ok(await _commonService.generateResponse(
                        true,
                        result.Entity,
                        "User role added successfully."
                    ));
                }
                else
                {
                    return Ok(await _commonService.generateResponse(
                        false,
                        null,
                        "Error while adding user role."
                    ));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, await _commonService.generateResponse(
                    false,
                    null,
                    $"Internal server error: {ex.Message}"
                ));
            }

        }


        private async Task<bool> SendResidentRegistrationEmailAsync(int residentId)
        {
            try
            {
                var residentInfo = await _context.residentInfo
                    .FirstOrDefaultAsync(r => r.resident_id == residentId);

                if (residentInfo == null)
                    return false;

                var userRole = await (
                    from role in _context.userRoles
                    join userDetails in _context.users on role.user_id equals userDetails.user_id
                    join societyDetails in _context.societies on role.society_id equals societyDetails.society_id
                    where role.user_id == residentInfo.user_id &&
                          (role.role_id == RoleConstants.Owner ||
                           role.role_id == RoleConstants.Tenant ||
                           role.role_id == RoleConstants.FamilyMember)
                    select new
                    {
                        Role = role,
                        User = userDetails,
                        Society = societyDetails
                    }
                ).FirstOrDefaultAsync();

                if (userRole == null)
                    return false;

                var user = userRole.User;
                var society = userRole.Society;

                // 🟢 Auto move from draft → pending
                if (residentInfo.verification_status?.ToLower() == "draft")
                {
                    residentInfo.verification_status = "pending";
                }

                // 🟢 Activate the user if needed
                if (!user.is_active)
                {
                    user.is_active = true;
                }

                // Save both in one go
                await _context.SaveChangesAsync();

                // 🟢 Send Email
                string subject = $"Welcome to {society.name} on Smart Society!";
                string loginLink = $"{_config["AppSettings:FrontendBaseUrl"]}/login";

                string body = $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h3>Dear {user.first_name},</h3>
                <p>Welcome to <strong>{society.name}</strong>! 🎉</p>
                <p>Your resident profile has been successfully registered on the Smart Society platform.</p>
                <p><b>Society Name:</b> {society.name}</p>
                <p><b>Registered Email:</b> {user.email}</p>
                <p>You can now log in using your registered email to manage your flat, family members, and documents.</p>

                <p>
                    <a href='{loginLink}'
                       style='background-color:#28a745;color:#fff;padding:10px 18px;
                              text-decoration:none;border-radius:4px;display:inline-block;'>
                       Log In to Smart Society
                    </a>
                </p>
                <br/>
                <p>Best regards,<br/><b>Smart Society Team</b></p>
            </div>";

                await _emailService.SendEmailAsync(user.email, subject, body);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendResidentRegistrationEmailAsync] Error: {ex.Message}");
                return false;
            }
        }

        private async Task SendSocietyRegistrationEmailAsync(UserModel admin, Societies society)
        {
            string subject = $"Welcome to Smart Society - {society.name} Registered Successfully!";
            string loginLink = $"{_config["AppSettings:FrontendBaseUrl"]}/login";

            string body = $@"
        <div style='font-family: Arial, sans-serif; color: #333;'>
            <h3>Dear {admin.first_name},</h3>
            <p>Your society <strong>{society.name}</strong> has been successfully registered on Smart Society.</p>
            <p><b>Society Code:</b> {society.society_code}</p>
            <p>You can now log in using your registered email <b>{admin.email}</b> 
            and update your profile for verification.</p>
            <p>
                <a href='{loginLink}' 
                   style='background-color:#007bff;color:#fff;padding:10px 18px;text-decoration:none;border-radius:4px;'>
                   Log In to Smart Society
                </a>
            </p>
            <br/>
            <p>Best regards,<br/><b>Smart Society Team</b></p>
        </div>";

            await _emailService.SendEmailAsync(admin.email, subject, body);
        }


        private async Task<bool> SendCommitteeRoleAssignmentEmailAsync(UserModel resident, Societies society, string roleName)
        {
            try
            {
                string subject = $"Smart Society - You have been assigned as {roleName} in {society.name}";
                string loginLink = $"{_config["AppSettings:FrontendBaseUrl"]}/login";

                string body = $@"
                            <div style='font-family: Arial, sans-serif; color: #333; line-height:1.6;'>
                                <h3>Dear {resident.first_name},</h3>
                                <p>Congratulations! You have been assigned the role of 
                                <strong>{roleName}</strong> in the society <strong>{society.name}</strong>.</p>

                                <p><b>Society Code:</b> {society.society_code}</p>
                                <p>You can log in using your registered email <b>{resident.email}</b> to view your role details.</p>

                                <p>
                                    <a href='{loginLink}' 
                                       style='background-color:#28a745;color:#fff;padding:10px 18px;text-decoration:none;border-radius:4px;'>
                                       Go to Smart Society
                                    </a>
                                </p>
                                <br/>
                                <p>Best regards,<br/><b>Smart Society Team</b></p>
                            </div>";

                await _emailService.SendEmailAsync(resident.email, subject, body);
                return true; // ✅ Email sent successfully
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendCommitteeRoleAssignmentEmailAsync] Error sending email to {resident.email}: {ex.Message}");
                return false; // ❌ Something went wrong
            }
        }

    }

}
