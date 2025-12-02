using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementAPI.Model
{
    [Table("family_member",Schema = "master")]
    public class FamilyMembers
    {
        [Key]
        public int family_member_id { get; set; }
        public int resident_id { get; set; }
        public string full_name { get; set; }
        public string relation { get; set; }
        public int age { get; set; }
        public string gender { get; set; }
        public string contact_number { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public bool is_co_owner { get; set; }
        public bool is_associate_member { get; set; }
        public bool is_nominee { get; set; }
    }

    
}
