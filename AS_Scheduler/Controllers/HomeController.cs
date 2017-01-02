﻿using Microsoft.AspNet.Identity;
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
using Scheduler.Models.Code_First;

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
            ViewBag.Announcements = db.Announcments.OrderByDescending(c => c.Id).ToList();
            return View();
        }

        public ActionResult ProfilePage(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                var userCheck = db.Users.Find(id);
                if (userCheck != null)
                {
                    var roleListCheck = new List<string>();
                    foreach (var role in userCheck.Roles)
                    {
                        var roleName = db.Roles.First(u => u.Id == role.RoleId);
                        roleListCheck.Add(roleName.Name);
                    }
                    ViewBag.RoleList = roleListCheck.ToList();

                    return View(userCheck);
                }

            }

            var user = db.Users.Find(User.Identity.GetUserId());
            var roleList = new List<string>();
            foreach (var role in user.Roles)
            {
                var roleName = db.Roles.First(u => u.Id == role.RoleId);
                roleList.Add(roleName.Name);
            }
            ViewBag.RoleList = roleList.ToList();

            return View(user);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Admin()
        {
            var adminUsers = new List<AdminUserListModels>();
            var regularUsers = new List<AdminUserListModels>();
            UserRolesHelper helper = new UserRolesHelper(db);

            var ryan = db.Users.Find("61cd4463-7283-4241-bfc5-04f0a4a11902").Id;
            var taylor = db.Users.Find("619856cf-24df-4ccf-bbba-1d38bab527a8").Id;
            var clarissa = db.Users.Find("dd1b1885-c104-4835-8c0a-19e75643d900").Id;

            foreach (var user in db.Users.Where(u => u.Id == ryan || u.Id == taylor || u.Id == clarissa))
            {
                var eachUser = new AdminUserListModels();
                eachUser.roles = new List<string>();
                eachUser.user = user;
                eachUser.roles = helper.ListUserRoles(user.Id).ToList();

                adminUsers.Add(eachUser);
            }
            foreach (var user in db.Users.Where(u => u.Id != ryan && u.Id != taylor && u.Id != clarissa))
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
            ViewBag.Announcements = db.Announcments.OrderByDescending(c => c.Id).ToList();
            return View();
        }

        public ActionResult ChapterStats(int id)
        {
            var chapter = db.Chapters.Find(id);
            ViewBag.ChapterName = chapter.ChapterName;
            ViewBag.ChapterYear = chapter.ChapterYear;
            ViewBag.GalleryPhotos = db.GalleryPhotos.Where(p => p.ChapterId == id).Count();
            ViewBag.Notes = db.Notes.Where(n => n.ChapterId == id).Count();
            ViewBag.ScheduledEvents = db.MyEvents.Where(e => e.ChapterId == id).Count();
            return View();
        }

        // Get: EditUserRoles
        [Authorize(Roles = "Administrator")]
        public ActionResult EditUserRoles(string id)
        {
            var user = db.Users.Find(id);
            UserRolesHelper helper = new UserRolesHelper(db);
            var model = new AdminUserViewModels();
            model.Name = user.DisplayName;
            model.Id = user.Id;
            model.SelectedRoles = helper.ListUserRoles(id).ToArray();
            model.Roles = new MultiSelectList(db.Roles, "Name", "Name", model.SelectedRoles);

            return View(model);
        }

        // Post: EditUserRoles
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult EditUserRoles(AdminUserViewModels model)
        {
            var user = db.Users.Find(model.Id);
            UserRolesHelper helper = new UserRolesHelper(db);

            foreach (var role in db.Roles.Select(r => r.Name).ToList())
            {
                helper.RemoveUserFromRole(user.Id, role);
            }

            if (model.SelectedRoles != null)
            {
                foreach (var role in model.SelectedRoles)
                {
                    helper.AddUserToRole(user.Id, role);
                }

                return RedirectToAction("Admin", "Home");
            }
            else
            {
                return RedirectToAction("Admin", "Home");
            }
        }

        // Post: CreateChapter
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateChapter([Bind(Include = "Id,ChapterName,ChapterYear,CurrentChapter")] Chapter chapter)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            db.Chapters.Add(chapter);
            db.SaveChanges();
            return RedirectToAction("Admin", "Home");
        }
        // Post: CreateChapter
        [Authorize(Roles = "Administrator")]
        public ActionResult MakeCurrentChapter(int id)
        {
            foreach (var item in db.Chapters)
            {
                item.CurrentChapter = false;
            }
            var chapter = db.Chapters.Find(id);
            chapter.CurrentChapter = true;
            db.SaveChanges();
            return RedirectToAction("Admin", "Home");
        }

        // Post: CreateAnnouncement
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateAnnouncement([Bind(Include = "Id,AuthorId,Title,Body,Created")] Announcement announcement)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            announcement.AuthorId = user.Id;
            announcement.Created = System.DateTime.Now;
            db.Announcments.Add(announcement);
            db.SaveChanges();
            return RedirectToAction("Admin", "Home");
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

        public ActionResult MySchedule()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            ViewBag.ChapterName = currentChapter.ChapterName;
            ViewBag.ChapterYear = currentChapter.ChapterYear;

            ViewBag.Day1 = db.MyEvents.Where(e => e.ChapterId == currentChapter.Id && e.DayId == 1 && e.AuthorId == user.Id).OrderBy(e => e.StartTime).ToList();
            ViewBag.Day2 = db.MyEvents.Where(e => e.ChapterId == currentChapter.Id && e.DayId == 2 && e.AuthorId == user.Id).OrderBy(e => e.StartTime).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToSchedule(List<int> Schedulize)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (Schedulize != null)
            {
                int count = Schedulize.Count();
                for (int i = 0; i < count; i++)
                {
                    var eVent = db.Events.Find(Schedulize[i]);
                    var myEvent = new MyEvent();
                    myEvent.CorrespondingEventId = eVent.Id;
                    myEvent.AuthorId = user.Id;
                    myEvent.ChapterId = eVent.ChapterId;
                    myEvent.DayId = eVent.DayId;
                    myEvent.StartTime = eVent.StartTime;
                    myEvent.EndTime = eVent.EndTime;
                    myEvent.EventName = eVent.EventName;
                    myEvent.Description = eVent.Description;
                    db.MyEvents.Add(myEvent);
                    db.SaveChanges();
                }
            }          

            return RedirectToAction("MySchedule", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromSchedule(List<int> Remove)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (Remove != null) 
            {
                int count = Remove.Count();
                for (int i = 0; i < count; i++)
                {
                    var myEvent = db.MyEvents.Find(Remove[i]);
                    db.MyEvents.Remove(myEvent);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("MySchedule", "Home");
        }

        public ActionResult Notes()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var chapters = new List<Chapter>();
            foreach (var chapter in db.Chapters)
            {
                if (user.Notes.Where(n => n.ChapterId == chapter.Id).Count() > 0)
                {
                    chapters.Add(chapter);
                }
            }
            ViewBag.Chapters = chapters.OrderByDescending(c => c.Id).ToList();
            ViewBag.Notes = user.Notes.OrderByDescending(n => n.Id).ToList();
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            ViewBag.ChapterName = currentChapter.ChapterName;
            ViewBag.ChapterYear = currentChapter.ChapterYear;
            return View();
        }

        // POST: Home/CreateNote
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNote([Bind(Include = "Id,AuthorId,Title,Body,Created,ChapterId")] Note note)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            if (ModelState.IsValid)
            {
                note.AuthorId = user.Id;
                note.ChapterId = currentChapter.Id;
                note.Created = System.DateTime.Now;
                db.Notes.Add(note);
                db.SaveChanges();
                return RedirectToAction("Notes", "Home");
            }
            return View(note);
        }

        // GET: Home/EditNote/5
        public ActionResult EditNote(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = db.Notes.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        // POST: Home/EditNote/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNote([Bind(Include = "Id,AuthorId,Title,Body,Created,ChapterId")] Note note)
        {
            if (ModelState.IsValid)
            {
                db.Notes.Attach(note);                
                db.Entry(note).Property("Title").IsModified = true;
                db.Entry(note).Property("Body").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Notes", "Home");
            }
            return View(note);
        }

        // GET: Home/DeleteNote/5
        public ActionResult DeleteNote(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = db.Notes.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        // POST: Home/DeleteNote/5
        [HttpPost, ActionName("DeleteNote")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteNoteConfirmed(int id)
        {
            Note note = db.Notes.Find(id);
            db.Notes.Remove(note);
            db.SaveChanges();
            return RedirectToAction("Notes", "Home");
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