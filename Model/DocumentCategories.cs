using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("document_categories", Schema = "master")]
    public class DocumentCategories
    {
        [Key]
        public int category_id { get; set; }
        public string category_name { get; set; }
        public string description { get; set; }
        public bool is_active { get; set; }
    }
}
