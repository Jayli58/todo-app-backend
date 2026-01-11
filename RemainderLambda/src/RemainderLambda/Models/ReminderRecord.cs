using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Models
{
    public class ReminderRecord
    {
        // public string ReminderId { get; set; }
        public required string UserId { get; set; }
        public required string TodoId { get; set; }
        public required string Email { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public long RemindAtEpoch { get; set; }
    }
}
