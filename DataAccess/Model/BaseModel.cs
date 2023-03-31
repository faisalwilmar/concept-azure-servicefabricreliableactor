using Newtonsoft.Json;
using System;

namespace DataAccess.Model
{
    public class BaseModel
    {
        [JsonProperty(propertyName: "id")]
        public string Id { get; set; }

        [JsonProperty(propertyName: "partitionKey")]
        public string PartitionKey { get; internal set; }

        [JsonProperty(propertyName: "createdDateUtc")]
        public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

        [JsonProperty(propertyName: "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(propertyName: "lastUpdatedDateUtc")]
        public DateTime LastUpdatedDateUtc { get; set; } = DateTime.UtcNow;

        [JsonProperty(propertyName: "lastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty(propertyName: "isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty(propertyName: "_etag")]
        public string Etag { get; internal set; }

        public static string GenerateDocumentType(Type t)
        {
            while (t.BaseType.Name != "BaseModel")
            {
                t = t.BaseType;
            }

            return t.Name;
        }
    }
}
