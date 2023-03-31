using System.Runtime.Serialization;

namespace DataAccess
{
    // Serializable object can be saved in the StateManager.
    [DataContract]
    public class UserInterest
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string[] Interests { get; set; } = System.Array.Empty<string>();
    }
}