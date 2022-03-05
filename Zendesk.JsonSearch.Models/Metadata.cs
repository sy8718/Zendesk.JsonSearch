//---------------------------------------------------------------------
// This is the class for metadata
// All configurations/settings/relationships of entities will be reflected in this class
//---------------------------------------------------------------------


using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zendesk.JsonSearch.Models
{
    public class Metadata
    {
        [JsonPropertyName("relationships")]
        public List<Relationship> Relationships { get; set; }
        [JsonPropertyName("entities")]
        public List<EntityMetadata> Entities { get; set; }
       
    }

    public class Relationship
    {
        [JsonPropertyName("fromentity")]
        public string FromEntity { get; set; }
        [JsonPropertyName("fromproperty")]
        public string FromProperty { get; set; }
        [JsonPropertyName("toentity")]
        public string ToEntity { get; set; }
        [JsonPropertyName("toproperty")]
        public string ToProperty { get; set; }
    }

    public class EntityMetadata
    {
        [JsonPropertyName("entitycode")]
        public int EntityCode { get; set; }

        [JsonPropertyName("filename")]
        public string FileName { get; set; }
        [JsonPropertyName("displayname")]
        public string DisplayName { get; set; }
        [JsonPropertyName("entityname")]
        public string EntityName { get; set; }
        [JsonPropertyName("nameproperty")]
        public string NameProperty { get; set; }
        [JsonPropertyName("properties")]
        public List<string> Properties { get; set; }
        [JsonPropertyName("indexingproperties")]
        public List<string> IndexingProperties { get; set; }
    }
}
