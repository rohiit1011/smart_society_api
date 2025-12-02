using SocietyManagementAPI.Model;

namespace SocietyManagementAPI.Interface
{
    public interface IResidentService
    {
        Task<object> RegisterResidentFlatAsync(List<ResidentFlat> flatDto);
        Task<object> fetchAllResidents(int societyId);
        Task<object> AddFamilyMembers(List<FamilyMembers> FamilyMembers);
        Task<object> AddResidentVehicles(List<ResidentVehicles> residentVehicles);
        Task<object> GetResidenFulltDetails(int residentId);

        Task<object> GetResidenDetails(int residentId);
        Task<object> GetResidenFlatDetails(int residentId);
        Task<object> GetResidenFamilyDetails(int residentId);
        Task<object> GetResidenVehiclesDetails(int residentId);


        Task<object> GetCommitteeMembers(int societyId);
    }
}
