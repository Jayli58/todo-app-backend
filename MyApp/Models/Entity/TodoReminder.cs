using Amazon.DynamoDBv2.DataModel;

namespace MyApp.Models.Entity
{
    [DynamoDBTable("TodoReminders")]
    public class TodoReminder
    {
        [DynamoDBHashKey]     // PK
        public required string UserId { get; set; }

        [DynamoDBRangeKey]    // SK
        public required string TodoId { get; set; }

        public required string Email { get; set; }
        public required string Title { get; set; }
        public string? Content { get; set; }
        public long RemindAtEpoch { get; set; }
    }
}
