namespace Backend.Models
{
    public class SchoolADUser : Common
    {
        public string? UserName { get; set; }

        public int? StudentId { get; set; }


        public User User { get; set; }
    }
}
