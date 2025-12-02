using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("resident_documents", Schema = "master")]
    public class ResidentDocuments
    {
        [Key]
        public int resident_document_id { get; set; }
        public int resident_id { get; set; }

        [ForeignKey("DocumentType")]
        public int document_type_id { get; set; }
        public string file_name { get; set; }
        public string file_path { get; set; }
        public string file_type { get; set; }
        public int uploaded_by { get; set; }
        public int updated_by { get; set; }
        public DateTime upload_date { get; set; }
        public DateTime updated_at { get; set; }
        public bool is_active { get; set; }

        public virtual DocumentType? DocumentType { get; set; }
    }
}
