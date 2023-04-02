using DataAccess.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using UserProfileService.Interfaces;

namespace UserProfileService
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class UserProfileService : Actor, IUserProfileService
    {
        private readonly string userInterestStateName = "UserInterestState";
        private readonly string DB = "Advertisement";
        private readonly string actorId;
        private readonly CosmosClient client;

        public UserProfileService(CosmosClient client, ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
            this.actorId = actorId.ToString();
            this.client = client;
        }

        public async Task AddInterests(string[] interests, CancellationToken cancellationToken)
        {
            var currentUserInterest = await this.StateManager.GetStateAsync<DataAccess.UserInterest>(userInterestStateName, cancellationToken);

            #region Add Interest
            var hashSetInterest = currentUserInterest.Interests.ToHashSet();

            foreach (var interest in interests)
            {
                hashSetInterest.Add(interest);
            }

            currentUserInterest.Interests = hashSetInterest.ToArray();
            #endregion

            await this.StateManager.AddOrUpdateStateAsync<DataAccess.UserInterest>(userInterestStateName, currentUserInterest, (k, v) => currentUserInterest, cancellationToken);

            await this.StateManager.SaveStateAsync(cancellationToken);
        }

        public async Task<string[]> GetInterests(CancellationToken cancellationToken)
        {
            var currentUserInterest = await this.StateManager.GetStateAsync<DataAccess.UserInterest>(userInterestStateName, cancellationToken);

            return currentUserInterest.Interests;
        }

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"Actor {actorId} activated.");

            DataAccess.UserInterest userInterest = new();

            using (var repo = new CosmosDbNoSqlRepository<DataAccess.Model.UserInterest>(DB, client))
            {
                var userInterestData = (await repo.GetAsync(p => p.Id == actorId, usePaging: true, pageSize: 1)).FirstOrDefault();
                if (userInterestData != null)
                {
                    userInterest.UserId = userInterestData.Id;
                    userInterest.UserName = userInterestData.UserName;
                    userInterest.Interests = userInterestData.Interests;
                } else
                {
                    userInterest.UserId = actorId;
                }
            }

            await this.StateManager.TryAddStateAsync(userInterestStateName, userInterest);
        }

        protected override async Task OnDeactivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"Actor {actorId} deactivating.");

            var currentUserInterest = await this.StateManager.GetStateAsync<DataAccess.UserInterest>(userInterestStateName, new CancellationToken());

            using (var repo = new CosmosDbNoSqlRepository<DataAccess.Model.UserInterest>(DB, client))
            {
                var userInterestData = (await repo.GetAsync(p => p.Id == actorId, usePaging: true, pageSize: 1)).FirstOrDefault();
                if (userInterestData != null)
                {
                    if (currentUserInterest != null)
                    {
                        userInterestData.Interests = currentUserInterest.Interests;
                        await repo.UpdateAsync(userInterestData.Id, userInterestData, actorId);
                    }
                }
                else
                {
                    userInterestData = new DataAccess.Model.UserInterest
                    {
                        Id = currentUserInterest.UserId,
                        UserName = currentUserInterest.UserName,
                        Interests = currentUserInterest.Interests
                    };

                    await repo.CreateAsync(userInterestData);

                }
            }

            await this.StateManager.TryRemoveStateAsync(userInterestStateName);
        }
    }
}
