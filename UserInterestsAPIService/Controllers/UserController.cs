using Microsoft.AspNetCore.Mvc;

namespace UserInterestsAPIService.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{userId}/interests")]
        public async Task<string[]> GetInterests(string userId)
        {
            return Summaries;
        }

        [HttpPatch]
        [Route("{userId}/interests")]
        public async Task AddInterests([FromBody] string[] interestsToAdd)
        {
            Summaries = Summaries.Concat(interestsToAdd).ToArray();
        }
    }
}