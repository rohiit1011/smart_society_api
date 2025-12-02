using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.Interface;
using SocietyManagementAPI.Model;
using System;

namespace SocietyManagementAPI.Service
{
    public class ResidentService : IResidentService
    {
        private readonly SocietyContext _context;
        private readonly ICommonService _commonService;

        public ResidentService(SocietyContext context, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        public async Task<object> fetchAllResidents(int societyId)
        {
            try
            {
                // Step 1: Get all resident-user pairs linked to the society
                var baseResidents = await (
                    from resident in _context.residentInfo
                    join user in _context.users on resident.user_id equals user.user_id
                    join ur in _context.userRoles on user.user_id equals ur.user_id
                    where ur.society_id == societyId
                    select new
                    {
                        resident,
                        user
                    }
                )
                .AsNoTracking()
                .ToListAsync();

                // Step 2: Group by resident to remove duplicates
                var distinctResidents = baseResidents
                    .GroupBy(r => r.resident.resident_id)
                    .Select(g => g.First())
                    .ToList();

                // Step 3: Project with Flats and Roles
                var result = distinctResidents.Select(r => new
                {
                    r.resident.resident_id,
                    r.resident.first_name,
                    r.resident.last_name,
                    r.resident.email,
                    r.resident.phone,
                    r.resident.gender,
                    r.resident.date_of_birth,
                    r.resident.aadhar_number,
                    r.resident.pan_number,
                    r.resident.verification_status,

                    r.user.user_id,
                    r.user.username,
                    r.user.is_active,

                    Flats = (
                        from f in _context.residentFlats
                        join wing in _context.societyWings
                            on f.wing_id equals wing.wing_id into wingGroup
                        from wing in wingGroup.DefaultIfEmpty()
                        where f.resident_id == r.resident.resident_id
                        select new
                        {
                            f.resident_id,
                            f.wing_id,
                            wing.wing_name,
                            f.flat_or_house_number,
                            f.ownership_type,
                            f.floor_number,
                            f.start_date,
                            f.end_date,
                            f.is_primary_resident
                        }
                    ).ToList(),

                    Roles = (
                        from ur in _context.userRoles
                        join role in _context.roles
                            on ur.role_id equals role.role_id into roleGroup
                        from role in roleGroup.DefaultIfEmpty()
                        where ur.user_id == r.user.user_id && ur.society_id == societyId
                        select new
                        {
                            ur.id,
                            ur.user_id,
                            ur.society_id,
                            role.role_id,
                            role.role_name,
                            role.role_code,
                            role.description,
                            role.is_active,
                            role.role_type,
                            role.created_at,
                            role.updated_at
                        }
                    ).ToList()
                }).ToList();

                // Step 4: Return Response
                if (result.Any())
                {
                    return await _commonService.generateResponse(true, result, "Data Found");
                }
                else
                {
                    return await _commonService.generateResponse(false, result, "Data Not Found");
                }
            }
            catch (Exception ex)
            {
                return await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<object> RegisterResidentFlatAsync(List<ResidentFlat> flatDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (flatDto == null || flatDto.Count == 0)
                    return await _commonService.generateResponse(false, null, "Invalid flat data.");

                var now = DateTime.UtcNow;

                 

                _context.residentFlats.AddRangeAsync(flatDto);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await _commonService.generateResponse(
                    true,
                    new { flatDto.First().resident_flat_id, flatDto.First().resident_id },
                    "Resident flat/ownership details saved successfully."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return await _commonService.generateResponse(false, null, $"An error occurred: {ex.Message}");
            }
        }
        public async Task<object> AddFamilyMembers(List<FamilyMembers> familyMembers)
        {
            if (familyMembers == null || !familyMembers.Any())
            {
                var response = await _commonService.generateResponse(false, null, "No family members provided.");
                return response;
            }

            try
            {
                var now = DateTime.UtcNow;
 

                await _context.familyMembers.AddRangeAsync(familyMembers);
                await _context.SaveChangesAsync();

                var result = new
                {
                    Count = familyMembers.Count,
                    message="success"
                };

                var response = await _commonService.generateResponse(true, result, $"{familyMembers.Count} family members added successfully.");
                return response;
            }
            catch (Exception ex)
            {
                var response = await _commonService.generateResponse(false, null, $"Error while saving family members: {ex.Message}");
                return response;
            }
        }

        public async Task<object> AddResidentVehicles(List<ResidentVehicles> residentVehicles)
        {
            if (residentVehicles == null || !residentVehicles.Any())
            {
                var response = await _commonService.generateResponse(false, null, "No vehicles provided.");
                return response;
            }

            try
            {
                var now = DateTime.UtcNow;


                await _context.residentVehicles.AddRangeAsync(residentVehicles);
                await _context.SaveChangesAsync();

                var result = new
                {
                    Count = residentVehicles.Count,
                    message = "success"
                };

                var response = await _commonService.generateResponse(true, result, $"{residentVehicles.Count} vehicle details added successfully.");
                return response;    
            }
            catch (Exception ex)
            {
                var response = await _commonService.generateResponse(false, null, $"Error while saving vehicle details : {ex.Message}");
                return response;
            }
        }

        public async Task<object> GetResidenFulltDetails(int residentId)
        {
            try
            {
                // 1️⃣ Fetch main resident info
                var resident = await _context.residentInfo
                    .Where(r => r.resident_id == residentId || r.user_id==residentId)
                    .FirstOrDefaultAsync();

                if (resident == null)
                {
                    return await _commonService.generateResponse(false, null, "Resident not found");
                }
                residentId = resident.resident_id; ///if usedid paased by end user then if matches assign proper resident id


                // 2️⃣ Fetch related data
                //var flats = await _context.residentFlats
                //    .Where(f => f.resident_id == residentId)
                //    .ToListAsync();

                var flats = await (from f in _context.residentFlats
                                   join w in _context.societyWings
                                     on f.wing_id equals w.wing_id into wingGroup
                                   from w in wingGroup.DefaultIfEmpty()
                                   where f.resident_id == residentId
                                   select new ResidentFlat
                                   {
                                       resident_flat_id = f.resident_flat_id,
                                       resident_id = f.resident_id,
                                       wing_id = f.wing_id,
                                       wingName = w != null ? w.wing_name : "--",

                                       ownership_type = f.ownership_type,
                                       start_date = f.start_date,
                                       end_date = f.end_date,
                                       is_primary_resident = f.is_primary_resident,
                                       floor_number = f.floor_number,
                                       flat_or_house_number = f.flat_or_house_number,
                                       share_certificate_no = f.share_certificate_no,
                                       carpet_area_sqft = f.carpet_area_sqft,
                                       monthly_maintenance = f.monthly_maintenance,
                                       tenant_maintenance = f.tenant_maintenance,
                                       sinking_fund = f.sinking_fund,
                                       owner_resident_id=f.owner_resident_id??0
                                   }).ToListAsync();


                var family = await _context.familyMembers
                    .Where(fm => fm.resident_id == residentId)
                    .ToListAsync();

                var vehicles = await _context.residentVehicles
                    .Where(v => v.resident_id == residentId)
                    .ToListAsync();

                var documents = await _context.residentDocuments
                    .Where(d => d.resident_id == residentId)
                    .ToListAsync();

                // 3️⃣ Combine all data into one structured object
                var result = new
                {
                    Resident = resident,
                    Flats = flats,
                    FamilyMembers = family,
                    Vehicles = vehicles,
                    Documents = documents
                };

                // 4️⃣ Return unified response
                return await _commonService.generateResponse(true, result, "Resident details fetched successfully.");
            }
            catch (Exception ex)
            {
                return await _commonService.generateResponse(false, null, $"Error fetching resident details: {ex.Message}");
            }
        }

        public async Task<object> GetResidenDetails(int residentId)
        {
            try
            {
                // 1️⃣ Fetch main resident info
                var resident = await _context.residentInfo
                    .Where(r => r.resident_id == residentId)
                    .FirstOrDefaultAsync();

                if (resident == null)
                {
                    return await _commonService.generateResponse(false, null, "Resident not found");
                }

                // 4️⃣ Return unified response
                return await _commonService.generateResponse(true, resident, "Resident details fetched successfully.");
            }
            catch (Exception ex)
            {
                return await _commonService.generateResponse(false, null, $"Error fetching resident details: {ex.Message}");
            }
        }

        public async Task<object> GetResidenFlatDetails(int residentId)
        {
            try
            {
                // 1️⃣ Fetch main resident info
                var resident = await _context.residentFlats
                    .Where(r => r.resident_id == residentId)
                    .FirstOrDefaultAsync();

                if (resident == null)
                {
                    return await _commonService.generateResponse(false, null, "Resident flats not found");
                }

                // 2️⃣ Fetch related data
                var flats = await _context.residentFlats
                    .Where(f => f.resident_id == residentId)
                    .ToListAsync();              
 

                // 4️⃣ Return unified response
                return await _commonService.generateResponse(true, flats, "Resident flats details fetched successfully.");
            }
            catch (Exception ex)
            {
                return await _commonService.generateResponse(false, null, $"Error fetching resident details: {ex.Message}");
            }
        }

        public async Task<object> GetResidenFamilyDetails(int residentId)
        {
            try
            {
                // 1️⃣ Fetch main resident info
                var resident = await _context.familyMembers
                    .Where(r => r.resident_id == residentId)
                    .FirstOrDefaultAsync();

                if (resident == null)
                {
                    return await _commonService.generateResponse(false, null, "Resident family not found");
                }


                
                var family = await _context.familyMembers
                    .Where(fm => fm.resident_id == residentId)
                    .ToListAsync();
                

                // 4️⃣ Return unified response
                return await _commonService.generateResponse(true, family, "Resident family details fetched successfully.");
            }
            catch (Exception ex)
            {
                return await _commonService.generateResponse(false, null, $"Error fetching resident details: {ex.Message}");
            }
        }

        public async Task<object> GetResidenVehiclesDetails(int residentId)
        {
            try
            {
                // 1️⃣ Fetch main resident info
                var resident = await _context.residentVehicles
                    .Where(r => r.resident_id == residentId)
                    .FirstOrDefaultAsync();

                if (resident == null)
                {
                    return await _commonService.generateResponse(false, null, "Resident vehicle not found");
                }
                 

                var vehicles = await _context.residentVehicles
                    .Where(v => v.resident_id == residentId)
                    .ToListAsync();
 

                // 4️⃣ Return unified response
                return await _commonService.generateResponse(true, vehicles, "Resident vehicle details fetched successfully.");
            }
            catch (Exception ex)
            {
                return await _commonService.generateResponse(false, null, $"Error fetching resident details: {ex.Message}");
            }
        }


        public async Task<object> GetCommitteeMembers(int societyId)
        {
            try
            {
                if (societyId <= 0)
                    return await _commonService.generateResponse(false, null, "Invalid Society Id");

                // Fetch all committee role IDs first
                var committeeRoleIds = await _context.roles
                    .Where(r => r.role_type.ToLower() == "committee")
                    .Select(r => r.role_id)
                    .ToListAsync();

                // Step 1: Get committee members
                var committeeMembers = await (
                    from resident in _context.residentInfo
                    join userRole in _context.userRoles on resident.user_id equals userRole.user_id
                    join role in _context.roles on userRole.role_id equals role.role_id
                    where userRole.society_id == societyId && committeeRoleIds.Contains(userRole.role_id)
                    select new
                    {
                        resident.resident_id,
                        resident.first_name,
                        resident.last_name,
                        resident.email,
                        resident.phone,
                        resident.society_id,
                        resident.user_id,
                        role.role_name
                    }
                ).ToListAsync();

                // Step 2: Attach flats for each resident
                var residentIds = committeeMembers.Select(x => x.resident_id).ToList();

                var flatsWithWing = await (
                         from flat in _context.residentFlats
                         join wing in _context.societyWings on flat.wing_id equals wing.wing_id
                         where residentIds.Contains(flat.resident_id)
                         select new
                         {
                             flat.resident_id,
                             flat.flat_or_house_number,
                             flat.wing_id,
                             wing.wing_name
                         }
                     ).ToListAsync();



                var result = committeeMembers.Select(c => new
                {
                    c.resident_id,
                    c.first_name,
                    c.last_name,
                    c.email,
                    c.phone,
                    c.society_id,
                    c.user_id,
                    c.role_name,
                    flats = flatsWithWing
                .Where(f => f.resident_id == c.resident_id)
                .Select(f => new
                {
                    f.wing_name,
                    f.flat_or_house_number
                })
                .ToList()
                }).ToList();

                if (result != null && result.Any())
                    return await _commonService.generateResponse(true, result, "Committee data found");
                else
                    return await _commonService.generateResponse(false, null, "Committee Not Found");
            }
            catch (Exception ex)
            {
                return await _commonService.generateResponse(false, null, $"Error fetching committee members: {ex.Message}");
            }
        }

    }

}
