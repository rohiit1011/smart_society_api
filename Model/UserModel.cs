using SocietyManagementAPI.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("users",Schema = "auth")]
    public class  UserModel : UserModelDTO
    {
        [Key]
        public int user_id { get; set; }
        public bool is_active { get; set; }
        public string password_hash { get; set; }
        public string username { get; set; }

        [NotMapped]
        public string society_code { get; set; } = null!;
        [NotMapped]
        public string society_email { get; set; } = null!;

    }
}
