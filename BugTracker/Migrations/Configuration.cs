namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.Classes;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BugTracker.Models.ApplicationDbContext";
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }
            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }
            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }
            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }
            ApplicationUser adminUser;
            if (!context.Users.Any(item => item.UserName == "admin@bugtracker.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.UserName = "admin@bugtracker.com";
                adminUser.Email = "admin@bugtracker.com";
                adminUser.LastName = "Admin";
                adminUser.FirstName = "Arshpreet";
                adminUser.DisplayName = "ArshpreetKaur";
                userManager.Create(adminUser, "Password-1");
            }
            else
            {
                adminUser = context.Users.FirstOrDefault(item => item.UserName == "admin@bugtracker.com");
            }

            if (!userManager.IsInRole(adminUser.Id, "Admin"))
            {
                userManager.AddToRole(adminUser.Id, "Admin");
            }

            ApplicationUser DemoAdminUser = new ApplicationUser();
            ApplicationUser DemoPMUser = new ApplicationUser();
            ApplicationUser DemoDeveloperUser = new ApplicationUser();
            ApplicationUser DemoSubmitterUser = new ApplicationUser();

            //demo admin
            if (!context.Users.Any(item => item.UserName == "demoadmin@admin.com"))
            {
                DemoAdminUser.UserName = "demoadmin@admin.com";
                DemoAdminUser.Email = "demoadmin@admin.com";
                DemoAdminUser.LastName = "DemoAdmin";
                DemoAdminUser.FirstName = "Arsh";
                DemoAdminUser.DisplayName = "Arsh";
                DemoAdminUser.Name = "DemoAdmin";


                userManager.Create(DemoAdminUser, "demoadmin@admin.com");
            }
            else
            {
                DemoAdminUser = context.Users.FirstOrDefault(item => item.UserName == "demoadmin@admin.com");
            }

            if (!userManager.IsInRole(DemoAdminUser.Id, "Admin"))
            {
                userManager.AddToRole(DemoAdminUser.Id, "Admin");
            }

            //demo Project Manager
            if (!context.Users.Any(item => item.UserName == "demoPM@pm.com"))
            {
                DemoPMUser.UserName = "demoPM@pm.com";
                DemoPMUser.Email = "demoPM@pm.com";
                DemoPMUser.LastName = "DemoPM";
                DemoPMUser.FirstName = "Arsh";
                adminUser.DisplayName = "Arsh";
                adminUser.Name = "DemoPm";


                userManager.Create(DemoPMUser, "demoPM@pm.com");
            }
            else
            {
                DemoPMUser = context.Users.FirstOrDefault(item => item.UserName == "demoPM@pm.com");
            }

            if (!userManager.IsInRole(DemoPMUser.Id, "Project Manager"))
            {
                userManager.AddToRole(DemoPMUser.Id, "Project Manager");
            }

            //demo developer
            if (!context.Users.Any(item => item.UserName == "demoDev@dev.com"))
            {
                DemoDeveloperUser.UserName = "demoDev@dev.com";
                DemoDeveloperUser.Email = "demoDev@dev.com";
                DemoDeveloperUser.LastName = "DemoDev";
                DemoDeveloperUser.FirstName = "Arsh";
                adminUser.DisplayName = "Arsh";
                adminUser.Name = "DemoDev";


                userManager.Create(DemoDeveloperUser, "demoDev@dev.com");
            }
            else
            {
                DemoDeveloperUser = context.Users.FirstOrDefault(item => item.UserName == "demoDev@dev.com");
            }

            if (!userManager.IsInRole(DemoDeveloperUser.Id, "Developer"))
            {
                userManager.AddToRole(DemoDeveloperUser.Id, "Developer");
            }

             //demo Submitter
            if (!context.Users.Any(item => item.UserName == "demoSubmitter@Sub.com"))
            {
                DemoSubmitterUser.UserName = "demoSubmitter@Sub.com";
                DemoSubmitterUser.Email = "demoSubmitter@Sub.com";
                DemoSubmitterUser.LastName = "DemoSubmitter";
                DemoSubmitterUser.FirstName = "Arsh";
                adminUser.DisplayName = "Arsh";
                adminUser.Name = "DemoSub";


                userManager.Create(DemoSubmitterUser, "demoSubmitter@Sub.com");
            }
            else
            {
                DemoSubmitterUser = context.Users.FirstOrDefault(item => item.UserName == "demoSubmitter@Sub.com");
            }

            if (!userManager.IsInRole(DemoSubmitterUser.Id, "Submitter"))
            {
                userManager.AddToRole(DemoSubmitterUser.Id, "Submitter");
            }


            context.TicketTypes.AddOrUpdate(x => x.Id,
              new Models.Classes.TicketType() { Id = 1, Name = "Bug Fixes" },
              new Models.Classes.TicketType() { Id = 2, Name = "Software Update" },
              new Models.Classes.TicketType() { Id = 3, Name = "Adding Helpers" },
              new Models.Classes.TicketType() { Id = 4, Name = "Database errors" });

            context.TicketPriorities.AddOrUpdate(x => x.Id,
                new Models.Classes.TicketPriority() { Id = 1, Name = "High" },
                new Models.Classes.TicketPriority() { Id = 2, Name = "Medium" },
                new Models.Classes.TicketPriority() { Id = 3, Name = "Low" },
                new Models.Classes.TicketPriority() { Id = 4, Name = "Urgent" });

            context.TicketStatuses.AddOrUpdate(x => x.Id,
                new Models.Classes.TicketStatus() { Id = 1, Name = "Finished" },
                new Models.Classes.TicketStatus() { Id = 2, Name = "Started" },
                new Models.Classes.TicketStatus() { Id = 3, Name = "Not Started" },
                new Models.Classes.TicketStatus() { Id = 4, Name = "In progress" });
            context.SaveChanges();
        }
    }}
