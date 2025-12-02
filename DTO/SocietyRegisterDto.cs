namespace SocietyManagementAPI.DTO
{
    public class SocietyRegisterDto
    {
        public string  name { get; set; } = null!;
        public string? society_type { get; set; } = "";
         public string email { get; set; } = null!; 
        public string city { get; set; } = null!;
        public string pincode { get; set; } = null!;
        public string? address { get; set; } = "";
        public string? phone { get; set; }
    }
}
