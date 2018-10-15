using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class AssignDevelopersTicketModel
    {
        public int TicketId { get; set; }
        public SelectList DeveloperList { get; set; }
        public string SelectedDeveloperId { get; set; }
        public string TicketName { get; set; }
    }
}