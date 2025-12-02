using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("society_maintenance_settings",Schema = "master")]
    public class SocietyMaintenanceSettings
    {
        [Key]
        public int setting_id { get; set; }
        public int society_id { get; set; }
        public string maintenance_type { get; set; }
        public decimal amount { get; set; }
        public int due_day { get; set; }
        public int penalty_after_days { get; set; }
        public decimal penalty_amount { get; set; }
        public string maintenance_frequency { get; set; }
        public bool is_active { get; set; }
        public int created_by { get; set; }
        public DateTime created_at { get; set; }
        public int updated_by { get; set; }
        public DateTime updated_at { get; set; }
    }
}
