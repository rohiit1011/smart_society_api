using SocietyManagementAPI.Interface;
using TokenDocsAPI.Models;
using static TokenDocsAPI.Models.ResponseStatusModel;
using static TokenDocsAPI.Models.SuccessMessageModel;

namespace SocietyManagementAPI.Service
{
    public class CommonService : ICommonService
    {
        ResponseStatusModel responseStatusModel=new ResponseStatusModel();
        SuccessMessageModel successMessageModel=new SuccessMessageModel();
        ErrorMessageModel errorMessageModel=new ErrorMessageModel();
        public Task<ResponseStatusModel> generateResponse(bool isSuccess, dynamic? responseData, string responseMessage )
        {
             if( isSuccess )
            {
                responseStatusModel.responseStatus=ResponseStatusEnum.Success;
                successMessageModel.successMessageStatus = ExecustionStatusEnum.Success;
                successMessageModel.successMessage = responseMessage;
                if(responseData != null) successMessageModel.responseData=responseData;

                responseStatusModel.successMessageResponse = successMessageModel;
                return Task.FromResult(responseStatusModel);
            }
            else
            {
                responseStatusModel.responseStatus = ResponseStatusEnum.Error;
                errorMessageModel.errorMessageStatus = ExecustionStatusEnum.Error;
                errorMessageModel.errorMessage = responseMessage;
                responseStatusModel.errorMessageResponse = errorMessageModel ;
                return Task.FromResult(responseStatusModel);
            }

        }
    }
}
