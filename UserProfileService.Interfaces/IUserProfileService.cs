using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace UserProfileService.Interfaces
{
    public interface IUserProfileService : IActor
    {
        /// <summary>
        /// Get User's Interest
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string[]> GetInterests(CancellationToken cancellationToken);

        /// <summary>
        /// Patch User's Interest.
        /// </summary>
        /// <param name="interests">Anything inside this array will be added to user's interest. Duplicated value will be ignored.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddInterests(string[] interests, CancellationToken cancellationToken);
    }
}
