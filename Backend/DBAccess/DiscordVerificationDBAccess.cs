using Backend.Data;

namespace Backend.DBAccess
{
    public class DiscordVerificationDBAccess
    {
        private readonly ApplicationDbContext _context;

        public DiscordVerificationDBAccess(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
