using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    //[Table("society", Schema = "master")]
    [Table("society", Schema = "master")]
    public class Societies
    {
        [Key]
        public int society_id { get; set; }
        public string society_code { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? city { get; set; }
        public string? pincode { get; set; }
        public string? address { get; set; }      
        public bool? is_active { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public string? verification_status { get; set; }

   
    }
}
