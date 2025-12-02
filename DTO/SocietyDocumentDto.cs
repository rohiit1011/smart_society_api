namespace SocietyManagementAPI.DTO
{
    public class SocietyDocumentDto
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public int? UploadedBy { get; set; }
    }
}
