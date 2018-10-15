using BugTracker.Models.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public DateTimeOffset Changed { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public int TicketId { get; set; }
        public virtual Tickets Ticket { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}