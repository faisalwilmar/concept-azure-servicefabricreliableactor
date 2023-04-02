using Microsoft.Azure.Cosmos;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace UserProfileService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDB");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid CosmosDB connection string in the Environment Variables");
                }
                CosmosClient client = new(connectionString);

                /* 
                 * Inject CosmosClient to be used in Actor on creation.
                 * reference: https://stackoverflow.com/questions/39647952/service-fabric-actor-service-dependency-injection-and-actor-events
                 * Set Garbage Collection to scan the actor every 60 second. if actor already idled for 60 second, it will be collected.
                 * reference: https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-actors-lifecycle
                */
                ActorRuntime.RegisterActorAsync<UserProfileService> (
                   (context, actorType) => new ActorService(context, actorType, (service, id) => new UserProfileService(client, service, id), settings: new ActorServiceSettings
                   {
                       ActorGarbageCollectionSettings =
                                new ActorGarbageCollectionSettings(60, 60)
                   }))
                    .GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
