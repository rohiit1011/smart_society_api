using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("resident_vehicles",Schema = "master")]
    public class ResidentVehicles
    {
        [Key]
        public int vehicle_id { get; set; }
        public int resident_id { get; set; }
        public string vehicle_number { get; set; }
        public string vehicle_type { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public string parking_slot { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
   
    }
}
