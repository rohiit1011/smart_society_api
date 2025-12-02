using TokenDocsAPI.Models;

namespace SocietyManagementAPI.Interface
{
    public interface ICommonService
    {
        Task<ResponseStatusModel> generateResponse(bool isSuccess, dynamic? responseData, string responseMessage);
    }
}
