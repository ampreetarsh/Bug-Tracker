using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using BugTracker.helper;
using BugTracker.Models;
using BugTracker.Models.Classes;
using Microsoft.AspNet.Identity;


namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db { get; set; }
        private UserRoleHelper UserRoleHelper { get; set; }
        public TicketsController()
        {
            db = new ApplicationDbContext();
            UserRoleHelper = new UserRoleHelper();
        }

        // GET: Tickets
        public ActionResult Index(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                return View(db.Tickets.Include(t => t.TicketPriority).Include(t => t.Project).Include(t => t.TicketStatus).Include(t => t.TicketType).Where(p => p.CreaterId == User.Identity.GetUserId()).ToList());
            }
            return View(db.Tickets.Include(t => t.TicketPriority).Include(t => t.Project).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList());
        }
        //Get UsreTickets
        public ActionResult UserTickets()
        {
            string userID = User.Identity.GetUserId();
            if (User.IsInRole("Submitter")) {
                var tickets = db.Tickets.Where(t => t.CreaterId == userID).Include(t => t.Creater).Include(t => t.Assignee).Include(t => t.Project);
                return View("Index", tickets.ToList());
            }
            if (User.IsInRole("Developer"))
            {
                var tickets = db.Tickets.Where(t => t.AssigneeId == userID).Include(t => t.Comments).Include(t => t.Creater).Include(t => t.Assignee).Include(t => t.Project);
                return View("Index", tickets.ToList());
            }
            if (User.IsInRole("Project Manager"))
            {
                return View(db.Tickets.Include(t => t.TicketPriority).Include(t => t.Project).Include(t=> t.Comments).Include(t => t.TicketStatus).Include(t => t.TicketType).Where(p => p.AssigneeId == userID).ToList());
            }

            return View("Index");
        }

        // Project Manger and Developer Projects
        [Authorize(Roles = "Developer,Project Manager")]
        public ActionResult ProjectManagerOrDeveloperTickets() {
            string userId = User.Identity.GetUserId();
            var ProjectMangerOrDeveloperId = db.Users.Where(p => p.Id == userId).FirstOrDefault();
            var projectsIds = ProjectMangerOrDeveloperId.Projects.Select(p => p.Id).ToList();
            var tickets1 = db.Tickets.Where(p => projectsIds.Contains(p.ProjectId)).ToList();
         return View("Index", tickets1);          
        }

        public ActionResult AssignDeveloper(int ticketId)
        {
            var model = new AssignDevelopersTicketModel();
            var ticket = db.Tickets.FirstOrDefault(p => p.Id == ticketId);
            var userRoleHelper = new UserRoleHelper();
            var users = userRoleHelper.UsersInRole("Developer");
            model.TicketId = ticketId;
            model.DeveloperList = new SelectList(users, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public ActionResult AssignDeveloper(AssignDevelopersTicketModel model)
        {
            var ticket = db.Tickets.FirstOrDefault(p => p.Id == model.TicketId);
            ticket.AssigneeId = model.SelectedDeveloperId;
            // Plug in your email service here to send an email.
            var user = db.Users.FirstOrDefault(p => p.Id == model.SelectedDeveloperId);
                var personalEmailService = new PersonalEmailService();
            var mailMessage = new MailMessage(
            WebConfigurationManager.AppSettings["emailto"],user.Email
                   );
                mailMessage.Body = "DB Has Some new Changes";
                mailMessage.Subject = "New Assigned Developer";
                mailMessage.IsBodyHtml = true;
                personalEmailService.Send(mailMessage);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Tickets.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            return View(tickets);
        }

        [HttpPost]
        [Authorize(Roles ="Admin, Project Manager,Submitter,Developer")]
        public ActionResult CreateComment(int id, string body)
        {
            var tickets = db.Tickets
               .Where(p => p.Id == id)
               .FirstOrDefault();
            var userId = User.Identity.GetUserId();
            var ProjectMangerOrDeveloperId = db.Users.Where(p => p.Id == userId).FirstOrDefault();
            var projectsIds = ProjectMangerOrDeveloperId.Projects.Select(p => p.Id).ToList();
            var projects = db.Tickets.Where(p => projectsIds.Contains(p.ProjectId)).ToList();

            if (tickets == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                ViewBag.ErrorMessage = "Comment is required";
                return View("Details", tickets);
            }

            if ((User.IsInRole("Admin")) || (User.IsInRole("Project Manager") && projects.Any(p => p.Id == id)) || (User.IsInRole("Submitter") && tickets.CreaterId == userId) || (User.IsInRole("Developer") && tickets.Assignee.Id == userId))
            {
                var comment = new TicketComment();
                comment.UserId = User.Identity.GetUserId();
                comment.TicketId = tickets.Id;
                comment.Created = DateTime.Now;
                comment.Comment = body;
                db.TicketComments.Add(comment);
                var user = db.Users.FirstOrDefault(p => p.Id == comment.UserId);
                var personalEmailService = new PersonalEmailService();
                var mailMessage = new MailMessage(
                WebConfigurationManager.AppSettings["emailto"], user.Email
                       );
                mailMessage.Body = "DB Has a new Comment";
                mailMessage.Subject = "Comment";
                mailMessage.IsBodyHtml = true;
                personalEmailService.Send(mailMessage);
                db.SaveChanges();
            }
            else if (User.Identity.IsAuthenticated) {
                ViewBag.ErrorMessage = "Sorry!! you are not allowed to comment.";
                return View("Details", tickets);
            }
            return RedirectToAction("Details", new {id });
        }

        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            ViewBag.AssigneeId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.CreaterId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create([Bind(Include = "Id,Name,Description,TicketTypeId,TicketPriorityId,ProjectId")] Tickets tickets)
        {   
            if (ModelState.IsValid)
            {
                if (tickets == null)
                {
                    return HttpNotFound();
                }   
                tickets.TicketStatusId = 3;
                db.Tickets.Add(tickets);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", tickets.TicketTypeId);
            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Submitter")]
        public ActionResult CreateAttachment(int ticketId,[Bind(Include = "Id,Description,TicketTypeId")] TicketAttachment ticketAttachment, HttpPostedFileBase image)
        {
            var tickets = db.Tickets
              .Where(p => p.Id == ticketId)
              .FirstOrDefault();
            var userId = User.Identity.GetUserId();
            var ProjectMangerOrDeveloperId = db.Users.Where(p => p.Id == userId).FirstOrDefault();
            var projectsIds = ProjectMangerOrDeveloperId.Projects.Select(p => p.Id).ToList();
            var projects = db.Tickets.Where(p => projectsIds.Contains(p.ProjectId)).ToList();
            if (ModelState.IsValid)
            {
                if (image == null)
                {
                    return HttpNotFound();
                }

                if (!ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    ViewBag.ErrorMessage = "Please upload an image";
                    
                }
                if ((User.IsInRole("Admin")) || (User.IsInRole("Project Manager") && projects.Any(p => p.Id == ticketId)) || (User.IsInRole("Submitter") && tickets.CreaterId == userId) || (User.IsInRole("Developer") && tickets.Assignee.Id == userId))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    ticketAttachment.FilePath = "/Uploads/" + fileName;
                    ticketAttachment.UserId = User.Identity.GetUserId();
                    ticketAttachment.Created = DateTime.Now;
                    ticketAttachment.TicketId = ticketId;
                    db.TicketAttachments.Add(ticketAttachment);
                    var user = db.Users.FirstOrDefault(p => p.Id == ticketAttachment.UserId);
                    var personalEmailService = new PersonalEmailService();
                    var mailMessage = new MailMessage(
                    WebConfigurationManager.AppSettings["emailto"], user.Email);
                    mailMessage.Body = "DB Has a new attachment";
                    mailMessage.Subject = "New Attachment";
                    mailMessage.IsBodyHtml = true;
                    personalEmailService.Send(mailMessage);
                    db.SaveChanges();
                }
                else if (User.Identity.IsAuthenticated)
                {
                    ViewBag.ErrorMessage = "Sorry! you are not allowed to attach document.";
                    return View("Details", tickets);
                }
                return RedirectToAction("Details",new { id = ticketId});
            }
            return View(ticketAttachment);
        }
        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Tickets.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssigneeId = new SelectList(db.Users, "Id", "DisplayName", tickets.AssigneeId);
            ViewBag.CreaterId = new SelectList(db.Users, "Id", "DisplayName", tickets.CreaterId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", tickets.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", tickets.TicketTypeId);
            return View(tickets);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,TicketTypeId,TicketPriorityId,CreaterId,TicketStatusId,ProjectId")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                var dateChanged = DateTimeOffset.Now;
                var changes = new List<TicketHistory>();
                var dbTicket = db.Tickets.First(p => p.Id == tickets.Id);
                dbTicket.Name = tickets.Name;
                dbTicket.Description = tickets.Description;
                dbTicket.TicketTypeId = tickets.TicketTypeId;
                dbTicket.Updated = dateChanged;
                dbTicket.TicketStatus = tickets.TicketStatusId;
                var originalValues = db.Entry(dbTicket).OriginalValues;
                var currentValues = db.Entry(dbTicket).CurrentValues;
                foreach (var property in originalValues.PropertyNames)
                {
                    var originalValue = originalValues[property]?.ToString();
                    var currentValue = currentValues[property]?.ToString();
                    if (originalValue != currentValue)
                    {
                        var history = new TicketHistory();
                        history.Changed = dateChanged;
                        history.NewValue = GetValueFromKey(property, currentValue);
                        history.OldValue = GetValueFromKey(property, originalValue);
                        history.Property = property;
                        history.TicketId = dbTicket.Id;
                        history.UserId = User.Identity.GetUserId();
                        changes.Add(history);
                    }
                }
                db.TicketHistories.AddRange(changes);
                if (dbTicket.AssigneeId != null)
                {
                    var user = db.Users.FirstOrDefault(p => p.Id == dbTicket.AssigneeId);
                    var personalEmailService = new PersonalEmailService();
                    var mailMessage = new MailMessage(
                    WebConfigurationManager.AppSettings["emailto"], user.Email
                           );
                    mailMessage.Body = "Ticket Has Some new Changes";
                    mailMessage.Subject = "Modified Ticket";
                    mailMessage.IsBodyHtml = true;
                    personalEmailService.Send(mailMessage);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssigneeId = new SelectList(db.Users, "Id", "DisplayName", tickets.AssigneeId);
            ViewBag.CreaterId = new SelectList(db.Users, "Id", "DisplayName", tickets.CreaterId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", tickets.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", tickets.TicketTypeId);
            return View(tickets);
        }

        private string GetValueFromKey(string propertyName, string key)
        {
            if (propertyName == "TicketTypeId")
            {
                return db.TicketTypes.Find(Convert.ToInt32(key)).Name;
            }

            return key;
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Tickets.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            return View(tickets);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tickets tickets = db.Tickets.Find(id);
            db.Tickets.Remove(tickets);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
