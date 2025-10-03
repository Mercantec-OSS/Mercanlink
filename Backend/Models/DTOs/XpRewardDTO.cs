namespace Backend.Models.DTOs
{
    public class XpRewardCreateDTO
    {
        public string Name { get; set; }

        public int Cooldown { get; set; }

        public int DailyLimit { get; set; }

        public int Reward { get; set; }
    }

    public class XpRewardUpdateDTO
    {
        public int Cooldown { get; set; }

        public int DailyLimit { get; set; }

        public int Reward { get; set; }
    }
}
