using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("roles",Schema = "auth")]
    public class Roles
    {
        [Key]
        public int role_id { get; set; }
        public string role_code { get; set; }
        public string role_name { get; set; }
        public string description { get; set; }
        public string role_type { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
