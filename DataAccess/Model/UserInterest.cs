using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class UserInterest : BaseModel
    {
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "interests")]
        public string[] Interests { get; set; }
    }
}
