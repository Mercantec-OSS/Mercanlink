using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class XpRewardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _sercretCode;

        public XpRewardController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _sercretCode = Environment.GetEnvironmentVariable("XPREWARDSECRETCODE") ?? config["XpRewardSecretCode"];
        }

        [HttpPost]
        public async Task<ActionResult> AddXpReward([FromBody] XpRewardDTO xpReward, string secretCode)
        {
            if (secretCode != _sercretCode)
            {
                return Unauthorized();
            }

            XpReward xpReward1 = new XpReward() { Name = xpReward.Name, Cooldown = xpReward.Cooldown, DailyLimit = xpReward.DailyLimit, Reward = xpReward.Reward };

            _context.XpRewards.Add(xpReward1);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<XpReward>>> GetAllXpReward(string secretCode)
        {
            if (secretCode != _sercretCode)
            {
                return Unauthorized();
            }

            var xpRewards = await _context.XpRewards.ToListAsync();

            return Ok(xpRewards);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateXpReward([FromBody] XpRewardDTO xpReward, int Id, string secretCode)
        {
            if (secretCode != _sercretCode)
            {
                return Unauthorized();
            }

            var oldXpReward = await _context.XpRewards.FirstOrDefaultAsync(x => x.Id == Id);

            if (oldXpReward == null) { return NotFound(); }

            oldXpReward.Name = xpReward.Name;
            oldXpReward.Cooldown = xpReward.Cooldown;
            oldXpReward.DailyLimit = xpReward.DailyLimit;
            oldXpReward.Reward = xpReward.Reward;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult<List<XpReward>>> DeleteXpReward(int Id, string secretCode)
        {
            if (secretCode != _sercretCode)
            {
                return Unauthorized();
            }
            var xpReward = await _context.XpRewards.FirstOrDefaultAsync(x => x.Id == Id);

            _context.XpRewards.Remove(xpReward);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
