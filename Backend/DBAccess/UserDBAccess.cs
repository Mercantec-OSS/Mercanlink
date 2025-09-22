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

        // Gets all users including their school ad user, discord user and their website user
        public async Task<List<User>> GetAllUsers()
        {
            var users = await _context.Users.Include(u => u.SchoolADUser).Include(u => u.DiscordUser).Include(u => u.WebsiteUser)
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        // Gets all users including their school ad user, discord user and their website user that have a discord id and a email
        public async Task<List<User>> GetAllUsersWithBothDiscordAndEmail()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser).Include(u => u.SchoolADUser)
                .Where(u => !string.IsNullOrEmpty(u.DiscordUser.DiscordId) && !string.IsNullOrEmpty(u.WebsiteUser.Email))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        // Gets all users including their school ad user, discord user and their website user that have a discord id and no email
        public async Task<List<User>> GetAllUsersWithDiscordOnly()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser).Include(u => u.SchoolADUser)
                .Where(u => !string.IsNullOrEmpty(u.DiscordUser.DiscordId) && string.IsNullOrEmpty(u.WebsiteUser.Email))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        // Gets all users including their school ad user, discord user and their website user that have an email and no discord id
        public async Task<List<User>> GetAllUsersWithEmailOnly()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser).Include(u => u.SchoolADUser)
                .Where(u => string.IsNullOrEmpty(u.DiscordUser.DiscordId) && !string.IsNullOrEmpty(u.WebsiteUser.Email))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        // Gets all users including their school ad user, discord user and their website user that have a discord id
        public async Task<List<User>> GetAllUsersWithDiscord()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser).Include(u => u.SchoolADUser)
                .Where(u => !string.IsNullOrEmpty(u.DiscordUser.DiscordId))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        // Gets all users including their school ad user, discord user and their website user that have an email
        public async Task<List<User>> GetAllUsersWithEmail()
        {
            var users = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser).Include(u => u.SchoolADUser)
                .Where(u => !string.IsNullOrEmpty(u.WebsiteUser.Email))
                .OrderBy(u => u.CreatedAt).ToListAsync();

            return users;
        }

        // Gets the user that matches the userid
        public async Task<User?> GetUser(string userId)
        {
            var user = await _context.Users.Include(u => u.SchoolADUser).Include(u => u.DiscordUser).Include(u => u.WebsiteUser)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }

        // Gets the user that matches the username
        public async Task<User?> GetUserFromUsername(string username)
        {
            var user = await _context.Users.Include(u => u.WebsiteUser).Include(u => u.DiscordUser).Include(u => u.SchoolADUser)
                .FirstOrDefaultAsync(u => u.SchoolADUser.UserName == username);

            return user;
        }

        // Adds a new user
        public async Task AddNewUser(User user)
        {
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }

        // Updates an user
        public async Task UpdateUser(User user)
        {
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        // Deletes an user
        public async Task DeleteUser(User user)
        {
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }

        // Gets the number of users
        public async Task<int> CountOfUsers()
        {
            var users = await GetAllUsers();

            return users.Count;
        }

        // Gets the number of users with discord only
        public async Task<int> CountOfUsersWithDiscordOnly()
        {
            var users = await GetAllUsersWithDiscordOnly();

            return users.Count;
        }

        // Gets the number of users with email only
        public async Task<int> CountOfUsersWithEmailOnly()
        {
            var users = await GetAllUsersWithEmailOnly();

            return users.Count;
        }

        // Gets the number of users with both discord and email
        public async Task<int> CountOfUsersWithBothDiscordAndEmail()
        {
            var users = await GetAllUsersWithBothDiscordAndEmail();

            return users.Count;
        }

        // Gets the number of users with discord
        public async Task<int> CountOfUsersWithDiscord()
        {
            var users = await GetAllUsersWithDiscord();

            return users.Count;
        }

        // Gets the number of users with email
        public async Task<int> CountOfUsersWithEmail()
        {
            var users = await GetAllUsersWithEmail();

            return users.Count;
        }

        // Checks if an username is in use
        public async Task<bool> CheckIfUsernameIsInUse(string username, string userId)
        {
            var existingUser = await _context.WebsiteUsers.FirstOrDefaultAsync(u => u.UserName == username && u.Id != userId);

            if (existingUser != null) { return true; }

            return false;
        }

        // Checks if multiple usernames are in use
        public async Task<List<SchoolADUser>> CheckIfMultipleUsernamesAreInUse(List<string> usernames)
        {
            var existingUsers = await _context.SchoolADUsers.Where(u => usernames.Contains(u.UserName)).ToListAsync();

            return existingUsers;
        }

        // Checks if an email is in use
        public async Task<bool> CheckIfEmailIsInUse(string email, string userId)
        {
            var existingUser = await _context.WebsiteUsers.FirstOrDefaultAsync(u => u.Email == email && u.Id != userId);

            if (existingUser != null) { return true; }

            return false;
        }


    }
}
