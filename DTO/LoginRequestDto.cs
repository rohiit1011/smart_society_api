namespace SocietyManagementAPI.DTO
{
    public class LoginRequestDto
    {
        public string email { get; set; }
        public string password { get; set; }
        public int role_id { get; set; }
    }
}
