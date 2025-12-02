using SocietyManagementAPI.DTO;
using SocietyManagementAPI.Model;

namespace SocietyManagementAPI.Interface
{
    public interface IMaintenanceService
    { 
            Task<List<PreviewBillDto>> PreviewGenerateAsync(MaintenanceGenerateRequest req);
            Task<GenerateResultDto> GenerateRunAsync(MaintenanceGenerateRequest req, int generatedByUserId);
            Task<List<MaintenanceBillRunDto>> FetchBillRunsAsync(int societyId);
            Task<List<MaintenanceBillDto>> FetchBillsForRunAsync(int runId);
            Task<bool> SendBillNoticeAsync(int billId);
            Task<List<MaintenanceHeads>> FetchAllHeadsAsync(int societyId);
        Task<MaintenanceHeads> AddHeadAsync(MaintenanceHeads maintenanceHeads);
        Task<bool> ApplyLatePenaltiesForDueRunsAsync();
        
    }
}
