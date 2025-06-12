using Backend.Data;

namespace Backend.DBAccess
{
    public class AuthDBAccess
    {
        private readonly ApplicationDbContext _context;

        public AuthDBAccess(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
