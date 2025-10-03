namespace Backend.Models
{
    public class XpReward
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Cooldown { get; set; }

        public int DailyLimit { get; set; }

        public int Reward { get; set; }
    }
}
