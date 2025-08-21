namespace Backend.Models
{
    public class DiscordUser : Common
    {
        public string? UserName { get; set; }

        public string? DiscordId { get; set; }

        public string? GlobalName { get; set; }

        public string? Discriminator { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Nickname { get; set; }

        public bool? IsBot { get; set; }

        public int? PublicFlags { get; set; }

        public DateTime? JoinedAt { get; set; }

        public bool? IsBoosting { get; set; }

        public bool IsActive { get; set; } = true;

        public int Experience { get; set; } = 0;

        public int Level { get; set; } = 1;


        public User User { get; set; }
    }
}
