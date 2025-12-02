using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace SocietyManagementAPI.Model
{
    [Table("document_types", Schema = "master")]
    public class DocumentType
    {
        [Key]
        public int document_type_id { get; set; }
        public string name { get; set; } = null!;
        public bool mandatory { get; set; } = true;
        public int category_id { get; set; }
        public int role_id { get; set; }
        public bool is_active { get; set; }
        [NotMapped]
        public string category_name { get; set; }="";
    }

    [Table("society_documents", Schema = "master")]
    public class SocietyDocument
    {
        [Key]
        public int document_id { get; set; }

        [ForeignKey("Society")]
        public int society_id { get; set; }

        [ForeignKey("DocumentType")]
        public int document_type_id { get; set; }

        [Required]
        [MaxLength(255)]
        public string file_name { get; set; } = null!;

        [Required]
        public string file_path { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string file_type { get; set; } = null!;

        public int? uploaded_by { get; set; }
        public int? updated_by { get; set; }

        public DateTime upload_date { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;

        public bool is_active { get; set; } = true;

        // Navigation properties
        public virtual DocumentType? DocumentType { get; set; }
        // Assuming you have Society entity
        //public virtual Society? Society { get; set; }
    }
}
