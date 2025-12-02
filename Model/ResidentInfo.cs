using SocietyManagementAPI.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("resident",Schema = "master")]
    public class ResidentInfo : UserModelDTO
    {
        [Key]
        public int resident_id { get; set; }
        public int user_id { get; set; }
        public string gender { get; set; }
        public DateTime date_of_birth { get; set; }
        public string aadhar_number { get; set; }
        public string pan_number { get; set; }
        public string photo_path { get; set; }
        public string? verification_status { get; set; }

        [NotMapped]
        public int society_id { get; set; }
        [NotMapped]
        public string password_hash { get; set; }
        [NotMapped]
        public string username { get; set; }
    }
}
