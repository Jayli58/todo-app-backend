using Amazon.DynamoDBv2.DataModel;

namespace MyApp.Models
{
    [DynamoDBTable("Todos")]
    public class TodoItem
    {
        [DynamoDBHashKey]           // Partition Key
        public string UserId { get; set; }

        [DynamoDBRangeKey]          // Sort Key
        public string TodoId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Completed { get; set; }
        public long? RemindTimestamp { get; set; }
        // 1 - Incomplete, 2 - Complete, 3 - Deleted
        public TodoStatus StatusCode { get; set; }
    }
}
