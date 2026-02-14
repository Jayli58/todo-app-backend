using Amazon.DynamoDBv2.DataModel;
using MyApp.Models.Enum;
using System.Text.Json.Serialization;

namespace MyApp.Models.Entity
{
    [DynamoDBTable("Todos")]
    public class TodoItem
    {
        [DynamoDBHashKey]           // Partition Key
        public required string UserId { get; set; }

        [DynamoDBRangeKey]          // Sort Key
        public required string TodoId { get; set; }
        public required string Title { get; set; }
        public string? Content { get; set; }
        // store lower case for case insensitive searchs
        // JsonIgnore to prevent it from being serialized into API responses
        [JsonIgnore]
        public string? TitleLower { get; set; }
        [JsonIgnore]
        public string? ContentLower { get; set; }
        public long? RemindTimestamp { get; set; }
        // 1 - Incomplete, 2 - Complete, 3 - Deleted
        public TodoStatus StatusCode { get; set; }
        public string? ActiveTodoId { get; set; }
    }
}
