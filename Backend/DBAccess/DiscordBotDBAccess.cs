using Backend.Data;
using Backend.Discord.Enums;
using Backend.DiscordServices.Services;
using Backend.Models;
using Backend.Models.DTOs;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace Backend.DBAccess
{
    public class DiscordBotDBAccess
    {
        private readonly ApplicationDbContext _context;

        public DiscordBotDBAccess(ApplicationDbContext context)
        {
            _context = context;
        }

        // Gets the discord user that matches the discord id
        public async Task<DiscordUser?> GetDiscordUser(string discordId)
        {
            // Tjek om Discord ID allerede er verificeret
            var user = await _context.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            return user;
        }

        // Get activity that matches the discord user id and activity name and the date
        public async Task<UserDailyActivity?> CheckTodaysActivity(string discordUserId, string activityName, DateTime today)
        {
            var dailyActivity = await _context.Set<UserDailyActivity>()
            .FirstOrDefaultAsync(a => a.DiscordUserId == discordUserId &&
                                     a.ActivityType == activityName &&
                                     a.Date == today);
            return dailyActivity;
        }

        // Checks if the user that matches the discord user id have gotten their daily login reward today
        public async Task<UserDailyActivity?> CheckIfDailyLoginXPIsRewarded(string discordUserId, DateTime today)
        {
            var dailyLoginActivity = await _context.Set<UserDailyActivity>()
            .FirstOrDefaultAsync(a => a.DiscordUserId == discordUserId &&
                                    a.ActivityType == XpActivityType.DailyLogin.GetName() &&
                                    a.Date == today);
            return dailyLoginActivity;
        }

        // Adds a new activity
        public async Task AddDailyActivity(UserDailyActivity dailyActivity)
        {
            _context.Set<UserDailyActivity>().Add(dailyActivity);

            await _context.SaveChangesAsync();
        }

        // Updates the activity that has already been saved
        public async Task UpdateDailyAcitivity(UserDailyActivity dailyActivity)
        {
            _context.Entry(dailyActivity).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        // Gets all activity that matches the discord user id and todays date
        public async Task<List<UserDailyActivity>> GetAllTodaysActivity(string discordUserId, DateTime today)
        {
            var dailyActivities = await _context.Set<UserDailyActivity>()
                .Where(a => a.DiscordUserId == discordUserId && a.Date == today)
                .ToListAsync();

            return dailyActivities;
        }

        // Updates the discord user that has already been saved
        public async Task UpdateUser(DiscordUser user)
        {
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        // Adds a new user
        public async Task AddUser(User newUser)
        {
            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();
        }

        // Gets the top x user according to their exp
        public async Task<List<DiscordUser>> GetTopUsers(int amount = 5)
        {
            var topUsers = await _context
                    .DiscordUsers.Where(u => u.IsBot == null || u.IsBot == false) // Ændret fra GetValueOrDefault
                    .OrderByDescending(u => u.Experience)
                    .Take(amount)
                    .ToListAsync();

            return topUsers;
        }

        // Gets the position you would have if you have x user exp
        public async Task<int> GetUserPosition(int userExperience)
        {
            var userPosition = await _context
                    .DiscordUsers.Where(u =>
                        (u.IsBot == null || u.IsBot == false) && u.Experience >= userExperience
                    )
                    .CountAsync();

            return userPosition;
        }
    }
}
