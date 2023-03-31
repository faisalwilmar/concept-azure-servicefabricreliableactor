using DataAccess;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using UserProfileService.Interfaces;

namespace UserProfileService
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class UserProfileService : Actor, IUserProfileService
    {
        private readonly string userInterestStateName = "UserInterestState";
        private readonly string actorId;

        public UserProfileService(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
            this.actorId = actorId.ToString();
        }

        public async Task AddInterests(string[] interests, CancellationToken cancellationToken)
        {
            var currentUserInterest = await this.StateManager.GetStateAsync<UserInterest>(userInterestStateName, cancellationToken);

            #region Add Interest
            var hashSetInterest = currentUserInterest.Interests.ToHashSet();

            foreach (var interest in interests)
            {
                hashSetInterest.Add(interest);
            }

            currentUserInterest.Interests = hashSetInterest.ToArray();
            #endregion

            await this.StateManager.AddOrUpdateStateAsync<UserInterest>(userInterestStateName, currentUserInterest, (k, v) => currentUserInterest, cancellationToken);

            await this.StateManager.SaveStateAsync(cancellationToken);
        }

        public async Task<string[]> GetInterests(CancellationToken cancellationToken)
        {
            var currentUserInterest = await this.StateManager.GetStateAsync<UserInterest>(userInterestStateName, cancellationToken);

            return currentUserInterest.Interests;
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"Actor {actorId} activated.");

            var userInterest = new UserInterest()
            {
                UserId = Guid.NewGuid().ToString()
            };

            return this.StateManager.TryAddStateAsync(userInterestStateName, userInterest);
        }

        protected override Task OnDeactivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"Actor {actorId} deactivating.");

            return this.StateManager.TryRemoveStateAsync(userInterestStateName);
        }
    }
}
