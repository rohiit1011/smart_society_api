using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("resident_flat", Schema = "master")]
    public class ResidentFlat
    {
        [Key]
        public int resident_flat_id { get; set; }
        public int resident_id { get; set; }
        public int? wing_id { get; set; }

        [NotMapped]
        public string wingName { get; set; } = "";
        public string? ownership_type { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public bool? is_primary_resident { get; set; } = true;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;

        public string? floor_number { get; set; }
        public string? flat_or_house_number { get; set; }
        public string? share_certificate_no { get; set; }
        public decimal? carpet_area_sqft { get; set; }
        public decimal? monthly_maintenance { get; set; }
        public decimal? tenant_maintenance { get; set; }
        public decimal? sinking_fund { get; set; }
        public int? owner_user_id { get; set; }
    }

}
