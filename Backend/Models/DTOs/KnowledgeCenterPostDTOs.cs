namespace Backend.Models.DTOs
{
    public class PostDTO
    {
        public string Type { get; set; }

        public string Author { get; set; }

        public string DiscordId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string LinkToPost { get; set; }
    }
}
