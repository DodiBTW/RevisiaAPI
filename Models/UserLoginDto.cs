namespace RevisiaAPI.Models
{
    public class UserLoginDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}
