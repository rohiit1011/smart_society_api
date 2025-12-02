using Microsoft.AspNetCore.Mvc;
using SocietyManagementAPI.DTO;
using SocietyManagementAPI.Interface;
using SocietyManagementAPI.Service;


namespace SocietyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocietyDocumentsController : ControllerBase
    {
        private readonly ISocietyDocumentService _service;
        private readonly CommonService _commonService;

        public SocietyDocumentsController(ISocietyDocumentService service, CommonService commonService)
        {
            _service = service;
            this._commonService = commonService;
        }

        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
        {
            try
            {
                var doc = await _service.UploadDocumentAsync(request.SocietyIdOrResidentId, request.DocumentTypeId, request.File, request.UploadedBy,request.UserRole);
                var response = await _commonService.generateResponse(true, doc, "Document uploaded successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("society-document-list")]
        public async Task<IActionResult> SocietyDocList([FromQuery] int societyId)
        { 
            try
            {
                var docs = await _service.GetDocumentsBySocietyAsync(societyId);
                var response = await _commonService.generateResponse(true, docs, "Document fetched successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }



        [HttpGet("resident-document-list")]
        public async Task<IActionResult> ResidentDocList([FromQuery] int residentId)
        {
            try
            {
                var docs = await _service.GetDocumentsByResidentAsync(residentId);
                var response = await _commonService.generateResponse(true, docs, "Document fetched successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(false, null, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("download/{documentId}")]
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            try
            {
                var (fileBytes, fileName) = await _service.DownloadDocumentAsync(documentId);

                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }



        [HttpDelete("delete/{documentId}")]
        public async Task<IActionResult> Delete(int documentId)
        {
            await _service.DeleteDocumentAsync(documentId);
            return Ok(new { message = "Document deleted successfully." });
        }
        [HttpGet("document-types")]
        public async Task<IActionResult> GetAllDocumentTypes([FromQuery] int roleId)
        {
            try
            {
                var documentTypes = await _service.GetAllDocumentTypesAsync(roleId);

                var response = await _commonService.generateResponse(
                    true,
                    documentTypes,
                    "Document types fetched successfully."
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = await _commonService.generateResponse(
                    false,
                    null,
                    $"An error occurred: {ex.Message}"
                );

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

    }

}
