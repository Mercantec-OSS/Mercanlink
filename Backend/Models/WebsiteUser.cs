namespace Backend.Models
{
    public class WebsiteUser : Common
    {
        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public bool EmailConfirmed { get; set; }


        public User User { get; set; }
    }
}
