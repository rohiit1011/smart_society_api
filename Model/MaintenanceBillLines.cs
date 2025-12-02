using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("maintenance_bill_lines",Schema = "master")]
    public class MaintenanceBillLines
    {
        [Key]
        public int bill_line_id { get; set; }
        public int bill_id { get; set; }
        public int maintenance_head_id { get; set; }
        public string description { get; set; }
        public decimal amount { get; set; }
        public decimal quantity { get; set; }
        public decimal unit_rate { get; set; }
        public decimal tax_percent { get; set; }
        public decimal tax_amount { get; set; }
        public decimal line_total { get; set; }
        public DateTime created_at { get; set; }
    }
}
