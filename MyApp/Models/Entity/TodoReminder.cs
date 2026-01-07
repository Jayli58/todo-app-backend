using Amazon.DynamoDBv2.DataModel;

namespace MyApp.Models.Entity
{
    [DynamoDBTable("TodoReminders")]
    public class TodoReminder
    {
        [DynamoDBHashKey]     // PK
        public string UserId { get; set; }

        [DynamoDBRangeKey]    // SK
        public string TodoId { get; set; }

        public string Email { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public long RemindAtEpoch { get; set; }
    }
}
