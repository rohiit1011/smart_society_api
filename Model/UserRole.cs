using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("user_roles",Schema = "auth")]
    public class UserRole
    {
        [Key]
        public int id { get; set; }
        public int user_id { get; set; }
        public int society_id { get; set; }
        public int role_id { get; set; }
        public DateTime assigned_at { get; set; }
    }
}
