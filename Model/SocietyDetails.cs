using SocietyManagementAPI.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("society_details", Schema = "master")]
    public class SocietyDetails
    {
        [Key]
        public int society_detail_id { get; set; }
        [ForeignKey("Society")]
        public int society_id { get; set; }
        public int establishment_year { get; set; }
        public string pan_number { get; set; }
        public string gst_number { get; set; }
        public string? registration_no { get; set; }
        public DateTime registration_date { get; set; }
        public string water_connection_type { get; set; }
        public string electricity_provider { get; set; }
        public bool has_lift { get; set; }
        public bool has_generator { get; set; }

        public int? updated_by { get; set; }
      
        public DateTime? updated_at { get; set; }

        [NotMapped]
        public SocietyRegisterDto societyRegisterDto { get; set; }
    }
}
