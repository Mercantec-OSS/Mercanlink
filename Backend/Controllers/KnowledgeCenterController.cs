using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTOs;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeCenterController : ControllerBase
    {
        private readonly DiscordBotService _discordBotService;

        public KnowledgeCenterController(DiscordBotService discordBotService)
        {
            _discordBotService = discordBotService;
        }

        [HttpPost]
        public async Task<ActionResult> AddNewPostForApproval([FromBody] PostDTO post)
        {
            try
            {
                await _discordBotService.KnowledgeCenterPostApproval(post);

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Der opstod en fejl under forbinelse med discordbot" });
            }
        }
    }
}
