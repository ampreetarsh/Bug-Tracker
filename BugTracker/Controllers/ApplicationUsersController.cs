﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.helper;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ApplicationUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ApplicationUsers
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        public ActionResult ChangeRole(string id)
        {
            var model = new UserRoleViewModel();
            var userRoleHelper = new UserRoleHelper();
            model.Id = id;
            model.Name = User.Identity.Name;
            var roles = userRoleHelper.GetAllRoles();
            var userRoles = userRoleHelper.GetUserRoles(id);
            model.Roles = new MultiSelectList(roles, "Name", "Name", userRoles);
            return View(model);
        }

        [HttpPost]
        public ActionResult ChangeRole(UserRoleViewModel model)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var user = userManager.FindById(model.Id);
            var userRoles = userManager.GetRoles(user.Id);
            foreach (var role in userRoles)
            {
                userManager.RemoveFromRole(user.Id, role);
            }
            foreach (var role in model.SelectedRoles)
            {
                userManager.AddToRole(user.Id, role);
            }
            //var signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            //signInManager.SignIn(user, isPersistent: false, rememberBrowser: false);
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
