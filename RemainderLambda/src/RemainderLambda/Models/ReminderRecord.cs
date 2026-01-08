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
        public string UserId { get; set; }
        public string TodoId { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public long RemindAtEpoch { get; set; }
    }
}
