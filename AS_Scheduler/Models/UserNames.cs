using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Scheduler.Models
{
    public class UserNames : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
                //var user = db.Users.Find(User.Identity.GetUserId());
                ViewBag.ProfilePic = user.ProfilePic;
                ViewBag.DisplayName = user.DisplayName;
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
                ViewBag.Company = user.Company;
                ViewBag.JobTitle = user.JobTitle;

                var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
                ViewBag.RecentAnnouncements = db.Announcements.Where(a => a.ChapterId == currentChapter.Id).OrderByDescending(a => a.Id).ToList();

                base.OnActionExecuting(filterContext);
            }
        }
    }
}