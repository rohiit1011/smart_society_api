using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("maintenance_bills",Schema = "master")]
    public class MaintenanceBills
    {
        [Key]
        public int bill_id { get; set; }
        public int bill_run_id { get; set; }
        public int resident_flat_id { get; set; }
        public int resident_id { get; set; }
        public int? user_id { get; set; } = 0;
        public string bill_no { get; set; }
        public DateTime bill_date { get; set; }
        public DateTime period_from { get; set; }
        public DateTime period_to { get; set; }
        public DateTime due_date { get; set; }
        public decimal total_amount { get; set; }
        public decimal total_tax { get; set; }
        public decimal total_payable { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
