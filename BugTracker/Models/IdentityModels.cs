using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using BugTracker.Models.Classes;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Project> Projects { get; set; }
        public string DisplayName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }
        [InverseProperty("Creater")]
        public virtual ICollection<Tickets> CreatedTickets { get; set; }
        [InverseProperty("Assignee")]
        public virtual ICollection<Tickets> AssignedTickets { get; set; }
        public ApplicationUser() {
            Projects = new HashSet<Project>();
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<BugTracker.Models.Classes.Project> Projects { get; set; }

        public System.Data.Entity.DbSet<BugTracker.Models.Classes.Tickets> Tickets { get; set; }

        public System.Data.Entity.DbSet<BugTracker.Models.Classes.TicketStatus> TicketStatuses{ get; set; }
        public System.Data.Entity.DbSet<BugTracker.Models.Classes.TicketPriority> TicketPriorities { get; set; }
        public System.Data.Entity.DbSet<BugTracker.Models.Classes.TicketType> TicketTypes { get; set; }

    }
}