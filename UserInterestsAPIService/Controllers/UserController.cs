using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using UserProfileService.Interfaces;

namespace UserInterestsAPIService.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{userId}/interests")]
        public async Task<string[]> GetInterests(string userId)
        {
            var actorId = new ActorId(userId);

            // Proxy for communicating with Actor Service
            var proxy = ActorProxy.Create<IUserProfileService>(actorId, new Uri("fabric:/Advertisement/UserProfileServiceActorService"));
            var interest = await proxy.GetInterests(new CancellationToken());
            return interest;
        }

        [HttpPatch]
        [Route("{userId}/interests")]
        public async Task AddInterests(string userId, [FromBody] string[] interestsToAdd)
        {
            var actorId = new ActorId(userId);

            // Another way for getting Proxy for communicating with Actor Service
            var proxy = ActorProxy.Create<IUserProfileService>(actorId, "Advertisement", "UserProfileServiceActorService");
            await proxy.AddInterests(interestsToAdd, new CancellationToken());
        }
    }
}