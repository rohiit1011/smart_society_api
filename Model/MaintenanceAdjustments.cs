using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("maintenance_adjustments",Schema = "master")]
    public class MaintenanceAdjustments
    {
        [Key]
        public int adj_id { get; set; }
        public int bill_id { get; set; }
        public string adj_type { get; set; }
        public decimal amount { get; set; }
        public string reason { get; set; }
        public int created_by { get; set; }
        public DateTime created_at { get; set; }
    }
}
