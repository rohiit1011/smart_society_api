using System.ComponentModel.DataAnnotations;

namespace SocietyManagementAPI.Model
{
    public class MembershipApplication
    {
        [Key]
        public int ApplicationId { get; set; }
        public string ApplicantName { get; set; } = "";
        public int Age { get; set; }
        public string Occupation { get; set; } = "";
        public decimal MonthlyIncome { get; set; }
        public string OfficeAddress { get; set; } = "";
        public string ResidentialAddress { get; set; } = "";
        public string FlatDetails { get; set; } = "";
        public string PropertyDetailsJson { get; set; } = "[]"; // JSON array for table
        public bool HasIndependentIncome { get; set; } = true;
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
    }
}
