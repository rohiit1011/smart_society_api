using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("society_wings", Schema = "master")]
    public class SocietyWings
    {
        [Key]
        public int wing_id { get; set; }
        public int society_id { get; set; }
        public string wing_name { get; set; }
        public int number_of_floors { get; set; }
        public int total_flats { get; set; }
        public int occupied_flats { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
    }
}
