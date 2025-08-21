using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.DBAccess
{
    public class UserDBAccess
    {
        private readonly ApplicationDbContext _context;

        public UserDBAccess(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsers()
        {
            var users = await _context.Users.Include(u => u.SchoolADUser).Include(u => u.DiscordUser).Include(u => u.WebsiteUser)
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        public async Task<List<User>> GetAllUsersWithBothDiscordAndEmail()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser)
                .Where(u => !string.IsNullOrEmpty(u.DiscordUser.DiscordId) && !string.IsNullOrEmpty(u.WebsiteUser.Email))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        public async Task<List<User>> GetAllUsersWithDiscordOnly()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser)
                .Where(u => !string.IsNullOrEmpty(u.DiscordUser.DiscordId) && string.IsNullOrEmpty(u.WebsiteUser.Email))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        public async Task<List<User>> GetAllUsersWithEmailOnly()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser)
                .Where(u => string.IsNullOrEmpty(u.DiscordUser.DiscordId) && !string.IsNullOrEmpty(u.WebsiteUser.Email))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        public async Task<List<User>> GetAllUsersWithDiscord()
        {
            var users = await _context.Users.Include(u => u.DiscordUser)
                .Where(u => !string.IsNullOrEmpty(u.DiscordUser.DiscordId))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        public async Task<List<User>> GetAllUsersWithEmail()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser)
                .Where(u => !string.IsNullOrEmpty(u.WebsiteUser.Email))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        public async Task<User?> GetUser(string userId)
        {
            var user = await _context.Users.Include(u => u.SchoolADUser).Include(u => u.DiscordUser).Include(u => u.WebsiteUser)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }

        public async Task<User?> GetUserFromUsername(string username)
        {
            var user = await _context.Users.Include(u => u.WebsiteUser).FirstOrDefaultAsync(u => u.SchoolADUser.UserName == username);

            return user;
        }

        public async Task AddNewUser(User user)
        {
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(User user)
        {
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(User user)
        {
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }

        public async Task<int> CountOfUsers()
        {
            var users = await GetAllUsers();

            return users.Count;
        }

        public async Task<int> CountOfUsersWithDiscordOnly()
        {
            var users = await GetAllUsersWithDiscordOnly();

            return users.Count;
        }

        public async Task<int> CountOfUsersWithEmailOnly()
        {
            var users = await GetAllUsersWithEmailOnly();

            return users.Count;
        }

        public async Task<int> CountOfUsersWithBothDiscordAndEmail()
        {
            var users = await GetAllUsersWithBothDiscordAndEmail();

            return users.Count;
        }

        public async Task<int> CountOfUsersWithDiscord()
        {
            var users = await GetAllUsersWithDiscord();

            return users.Count;
        }

        public async Task<int> CountOfUsersWithEmail()
        {
            var users = await GetAllUsersWithEmail();

            return users.Count;
        }

        public async Task<bool> CheckIfUsernameIsInUse(string username, string userId)
        {
            var existingUser = await _context.WebsiteUsers.FirstOrDefaultAsync(u => u.UserName == username && u.Id != userId);

            if (existingUser != null) { return true; }

            return false;
        }

        public async Task<List<SchoolADUser>> CheckIfMultipleUsernamesAreInUse(List<string> usernames)
        {
            var existingUsers = await _context.SchoolADUsers.Where(u => usernames.Contains(u.UserName)).ToListAsync();

            return existingUsers;
        }

        public async Task<bool> CheckIfEmailIsInUse(string email, string userId)
        {
            var existingUser = await _context.WebsiteUsers.FirstOrDefaultAsync(u => u.Email == email && u.Id != userId);

            if (existingUser != null) { return true; }

            return false;
        }


    }
}
