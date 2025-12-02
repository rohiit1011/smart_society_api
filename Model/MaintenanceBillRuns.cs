using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("maintenance_bill_runs",Schema = "master")]
    public class MaintenanceBillRuns
    {
        [Key]
        public int bill_run_id { get; set; }
        public int society_id { get; set; }
        public DateTime period_from { get; set; }
        public DateTime period_to { get; set; }
        public string frequency { get; set; }
        public DateTime generated_at { get; set; }
        public int generated_by { get; set; }

        public decimal total_amount { get; set; }
        public string status { get; set; }
        public string? notes { get; set; } = "";
    }
}
