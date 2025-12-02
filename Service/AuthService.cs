using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.DTO;
using TokenDocsAPI.Models;

namespace SocietyManagementAPI.Service
{
    public class AuthService
    {
        private readonly SocietyContext _context;
        private readonly CommonService _commonService;

        public AuthService(SocietyContext context, CommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        public async Task<ResponseStatusModel> LoginAsync(LoginRequestDto model)
        {
            if (string.IsNullOrEmpty(model.email) || string.IsNullOrEmpty(model.password) || model.role_id <= 0)
            {
                return await _commonService.generateResponse(false, null, "Email, Password and Role ID are required.");
            }

            // 🔍 Step 1: Check if user exists
            var user = await _context.users.FirstOrDefaultAsync(u => u.email == model.email || u.username == model.email);
            if (user == null)
            {
                return await _commonService.generateResponse(false, null, "Invalid email or password.");
            }

            // 🔐 Step 2: Compare password (plain text for now)
            if (user.password_hash != model.password)
            {
                return await _commonService.generateResponse(false, null, "Invalid email or password.");
            }

            // 🧩 Step 3: Check if role assigned to this user
            var userRole = await _context.userRoles
                .Where(r => r.user_id == user.user_id && r.role_id == model.role_id)
                .Select(r => new { r.role_id, r.society_id, r.assigned_at })
                .FirstOrDefaultAsync();

            if (userRole == null)
            {
                return await _commonService.generateResponse(false, null, "Role not assigned to this user.");
            }
            // 🔍 Step 4: Get verification status from society table if role_id is 1 or 2
            string? verificationStatus = null;

            if (userRole.role_id == 1 || userRole.role_id == 2) // 1: Admin, 2: Super Admin
            {
                var society = await _context.societies
                    .Where(s => s.society_id == userRole.society_id)
                    .Select(s => s.verification_status)
                    .FirstOrDefaultAsync();

                verificationStatus = society; // can be "Verified", "Pending", etc.
            }

            // 🏁 Step 5: Prepare response object
            var responseData = new
            {
                user.user_id,
                user.first_name,
                user.last_name,
                user.middle_name,
                user.email,
                user.phone,
                userRole.role_id,
                userRole.society_id,
                  userRole.assigned_at,
                 verificationStatus,
                LoginStatus = "Login Successful"
            };

            return await _commonService.generateResponse(true, responseData, "Login successful.");
        }

    }
}
