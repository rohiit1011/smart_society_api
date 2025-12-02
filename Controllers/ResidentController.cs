using Microsoft.AspNetCore.Mvc;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.Interface;
using SocietyManagementAPI.Model;
using SocietyManagementAPI.Service;

namespace SocietyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResidentController : ControllerBase
    {

        private readonly IResidentService _residentService;

        public ResidentController(IResidentService residentService)
        {
            _residentService = residentService;
        }




        [HttpGet("fetchAllResidents")]
        public async Task<IActionResult> GetSocieties([FromQuery] int societyID)
        {
            var response = await _residentService.fetchAllResidents(societyID);
            return Ok(response);

        }

        [HttpPost("AddFamilyMembers")]
        public async Task<IActionResult> AddFamilyMembers([FromBody] List<FamilyMembers> FamilyMembers)
        {
            var response = await _residentService.AddFamilyMembers(FamilyMembers);
            return Ok(response);

        }



        [HttpPost("AddResidentVehicle")]
        public async Task<IActionResult> AddResidentVehicle([FromBody] List<ResidentVehicles> residentVehicles)
        {
            var response = await _residentService.AddResidentVehicles(residentVehicles);
            return Ok(response);

        }

        [HttpPost("submit-resident-flat-details")]
        public async Task<IActionResult> RegisterResidentFlatAsync([FromBody] List<ResidentFlat> flatDto)
        {
            var response = await _residentService.RegisterResidentFlatAsync(flatDto);
            return Ok(response);
        }

        [HttpGet("get-resident-full-details")]
        public async Task<IActionResult> GetResidenFulltDetails([FromQuery] int residentId)
        {
            var response = await _residentService.GetResidenFulltDetails(residentId);
            return Ok(response);
        }

        [HttpGet("get-committee-member")]
        public async Task<IActionResult> GetCommitteeMembers([FromQuery] int societyId)
        {
            var response = await _residentService.GetCommitteeMembers(societyId);
            return Ok(response);
        }

        [HttpGet("get-resident-details")]
        public async Task<IActionResult> GetResidentDetails([FromQuery] int residentId)
        {
            var response = await _residentService.GetResidenDetails(residentId);
            return Ok(response);
        }

        [HttpGet("get-resident-flat-details")]
        public async Task<IActionResult> GetResidenFlatDetails([FromQuery] int residentId)
        {
            var response = await _residentService.GetResidenFlatDetails(residentId);
            return Ok(response);
        }

        [HttpGet("get-resident-family-details")]
        public async Task<IActionResult> GetResidenFamilyDetails([FromQuery] int residentId)
        {
            var response = await _residentService.GetResidenFamilyDetails(residentId);
            return Ok(response);
        }


        [HttpGet("get-resident-vehicle-details")]
        public async Task<IActionResult> GetResidenVehiclesDetails([FromQuery] int residentId)
        {
            var response = await _residentService.GetResidenVehiclesDetails(residentId);
            return Ok(response);
        }

    }
}
