using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("maintenance_heads", Schema = "master")]
    public class MaintenanceHeads
    {
        [Key]
        public int maintenance_head_id { get; set; }
        public int society_id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string calc_type { get; set; }
        public bool is_mandatory { get; set; }
        public decimal default_amount { get; set; }
        public decimal? default_percentage { get; set; }
        public bool is_active { get; set; }
        public int created_by { get; set; }
        public DateTime created_at { get; set; }
        public int updated_by { get; set; }
        public DateTime updated_at { get; set; }
    }
}
