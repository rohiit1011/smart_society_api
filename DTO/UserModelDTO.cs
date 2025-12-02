using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.DTO
{
    public abstract class UserModelDTO
    {
       
        public string email { get; set; }
        public string phone { get; set; }
        //public string full_name { get; set; }

        public string? first_name { get; set; }
        public string? middle_name { get; set; }
        public string? last_name { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

        [NotMapped]
        public string loginLink { get; set; } = "";

        [NotMapped]
        public int role_id { get; set; }
    }
}
