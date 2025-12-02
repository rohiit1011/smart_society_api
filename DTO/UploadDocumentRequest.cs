using Microsoft.AspNetCore.Mvc;

namespace SocietyManagementAPI.DTO
{
    public class UploadDocumentRequest
    {
        [FromForm(Name = "societyIdOrResidentId")]
        public int SocietyIdOrResidentId { get; set; }

        [FromForm(Name = "userRole")]
        public string UserRole { get; set; }

        [FromForm(Name = "documentTypeId")]
        public int DocumentTypeId { get; set; }

        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = null!;

        [FromForm(Name = "uploadedBy")]
        public int UploadedBy { get; set; }
    }

}
