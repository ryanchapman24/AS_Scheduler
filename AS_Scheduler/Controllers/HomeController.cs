using Microsoft.AspNet.Identity;
using Scheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Net;
using System.IO;
using Microsoft.AspNet.Identity.Owin;
using Scheduler.Models.Helpers;

namespace Scheduler.Controllers
{
    [Authorize]
    public class HomeController : UserNames
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Admin()
        {
            var admins = new List<ApplicationUser>();
            var regulars = new List<ApplicationUser>();
            var adminUsers = new List<AdminUserListModels>();
            var regularUsers = new List<AdminUserListModels>();
            UserRolesHelper helper = new UserRolesHelper(db);
            foreach (var x in db.Users)
            {
                foreach (var role in x.Roles)
                {
                    if (role.RoleId == "e6b05319-7f68-4d67-90a7-764c2ea1bef2")
                    {
                        admins.Add(x);
                    }
                    else
                    {
                        regulars.Add(x);
                    }
                }
            }
            foreach (var user in admins)
            {
                var eachUser = new AdminUserListModels();
                eachUser.roles = new List<string>();
                eachUser.user = user;
                eachUser.roles = helper.ListUserRoles(user.Id).ToList();

                adminUsers.Add(eachUser);
            }
            foreach (var user in regulars)
            {
                var eachUser = new AdminUserListModels();
                eachUser.roles = new List<string>();
                eachUser.user = user;
                eachUser.roles = helper.ListUserRoles(user.Id).ToList();

                regularUsers.Add(eachUser);
            }
            ViewBag.Admins = adminUsers.OrderBy(a => a.user.FirstName).ToList();
            ViewBag.Users = regularUsers.OrderBy(a => a.user.FirstName).ToList();
            ViewBag.Chapters = db.Chapters.OrderByDescending(c => c.Id).ToList();
            return View();
        }

        public ActionResult Agenda()
        {
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            ViewBag.ChapterName = currentChapter.ChapterName;
            ViewBag.ChapterYear = currentChapter.ChapterYear;

            ViewBag.Day1 = db.Events.Where(e => e.ChapterId == currentChapter.Id && e.DayId == 1).OrderBy(e => e.StartTime).ToList();
            ViewBag.Day2 = db.Events.Where(e => e.ChapterId == currentChapter.Id && e.DayId == 2).OrderBy(e => e.StartTime).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendMessage(string name, string email, string subject, string message)
        {
            object returnValue = null;
            try
            {
                var mC = db.Users.FirstOrDefault(u => u.Id == "619856cf-24df-4ccf-bbba-1d38bab527a8");
                var mA = db.Users.FirstOrDefault(u => u.Id == "dd1b1885-c104-4835-8c0a-19e75643d900");
                var userToNotify1 = await UserManager.FindByNameAsync(mC.Email);
                var userToNotify2 = await UserManager.FindByNameAsync(mA.Email);

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link          
                //var callbackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(userToNotify1.Id, subject, "NAME: " + name + "<br /><br />" + "EMAIL: " + email + "<br /><br />" + "MESSAGE: " + message);
                await UserManager.SendEmailAsync(userToNotify2.Id, subject, "NAME: " + name + "<br /><br />" + "EMAIL: " + email + "<br /><br />" + "MESSAGE: " + message);

                returnValue = new { Message = "Success! " + "<i class='fa fa-check'></i>" };
            }
            catch (Exception)
            {
                returnValue = new { Message = "Error Sending Email" };
            }

            return Json(returnValue);
        }
    }
}