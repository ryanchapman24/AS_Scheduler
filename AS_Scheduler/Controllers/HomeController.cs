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
using Scheduler.Models.Code_First;
using System.Web.Security;

namespace Scheduler.Controllers
{
    [Authorize(Roles = "Administrator, User")]
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
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            var chapters = new List<Chapter>();
            foreach (var chapter in db.Chapters)
            {
                if (db.GalleryPhotos.Where(p => p.ChapterId == chapter.Id && p.Published == true).Count() > 0)
                {
                    chapters.Add(chapter);
                }
            }
            ViewBag.Chapters = chapters.OrderByDescending(c => c.Id).ToList();
            ViewBag.Photos = db.GalleryPhotos.Where(p => p.Published == true).OrderByDescending(n => n.Id).ToList();
            ViewBag.Announcements = db.Announcements.Where(a => a.ChapterId == currentChapter.Id).OrderByDescending(c => c.Id).ToList();
            ViewBag.CurrentChapterName = currentChapter.ChapterName;
            ViewBag.CurrentChapterYear = currentChapter.ChapterYear;
            ViewBag.CurrentAddressLine1 = currentChapter.AddressLine1;
            ViewBag.CurrentAddressLine2 = currentChapter.AddressLine2;
            ViewBag.CurrentLatitude = currentChapter.Latitude;
            ViewBag.CurrentLongitude = currentChapter.Longitude;
            ViewBag.CurrentCenter = currentChapter.Latitude + "," + currentChapter.Longitude;
            return View();
        }

        public ActionResult ProfilePage(string id)
        {
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            ViewBag.ChapterName = currentChapter.ChapterName;
            ViewBag.ChapterYear = currentChapter.ChapterYear;
            var now = System.DateTime.Now;
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
                    var chapters = new List<Chapter>();
                    foreach (var chapter in db.Chapters)
                    {
                        if (userCheck.GalleryPhotos.Where(p => p.ChapterId == chapter.Id).Count() > 0)
                        {
                            chapters.Add(chapter);
                        }
                    }
                    ViewBag.RoleList = roleListCheck.ToList();
                    ViewBag.UpcomingEvents = db.MyEvents.Where(e => e.AuthorId == userCheck.Id && e.ChapterId == currentChapter.Id && e.StartTime > now).OrderBy(e => e.StartTime).ToList();
                    ViewBag.Chapters = chapters.OrderByDescending(c => c.Id).ToList();
                    ViewBag.Photos = userCheck.GalleryPhotos.OrderByDescending(n => n.Id).ToList();
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
            var myChapters = new List<Chapter>();
            foreach (var chapter in db.Chapters)
            {
                if (user.GalleryPhotos.Where(p => p.ChapterId == chapter.Id).Count() > 0)
                {
                    myChapters.Add(chapter);
                }
            }
            ViewBag.RoleList = roleList.ToList();
            ViewBag.UpcomingEvents = db.MyEvents.Where(e => e.AuthorId == user.Id && e.ChapterId == currentChapter.Id).OrderByDescending(e => e.StartTime).ToList();
            ViewBag.Chapters = myChapters.OrderByDescending(c => c.Id).ToList();
            ViewBag.Photos = user.GalleryPhotos.OrderByDescending(n => n.Id).ToList();
            return View(user);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Admin()
        {
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            ViewBag.ChapterName = currentChapter.ChapterName;
            ViewBag.ChapterYear = currentChapter.ChapterYear;
            var adminUsers = new List<AdminUserListModels>();
            var regularUsers = new List<AdminUserListModels>();
            var blockedUsers = new List<AdminUserListModels>();
            UserRolesHelper helper = new UserRolesHelper(db);

            foreach (var user in db.Users.Where(u => u.Roles.Any(r => r.RoleId == "e6b05319-7f68-4d67-90a7-764c2ea1bef2")))
            {
                var eachUser = new AdminUserListModels();
                eachUser.roles = new List<string>();
                eachUser.user = user;
                eachUser.roles = helper.ListUserRoles(user.Id).ToList();

                adminUsers.Add(eachUser);
            }
            foreach (var user in db.Users.Where(u => u.Roles.Where(r => r.RoleId == "e6b05319-7f68-4d67-90a7-764c2ea1bef2").Count() == 0).Where(u => u.Roles.Where(r => r.RoleId == "25dfc3c4-229b-42a5-abe0-2f0016b79aa1").Count() == 1))
            {
                var eachUser = new AdminUserListModels();
                eachUser.roles = new List<string>();
                eachUser.user = user;
                eachUser.roles = helper.ListUserRoles(user.Id).ToList();

                regularUsers.Add(eachUser);
            }
            foreach (var user in db.Users.Where(u => u.Roles.Where(r => r.RoleId == "e6b05319-7f68-4d67-90a7-764c2ea1bef2" || r.RoleId == "25dfc3c4-229b-42a5-abe0-2f0016b79aa1").Count() == 0))
            {
                var eachUser = new AdminUserListModels();
                eachUser.roles = new List<string>();
                eachUser.user = user;
                eachUser.roles = helper.ListUserRoles(user.Id).ToList();

                blockedUsers.Add(eachUser);
            }
            ViewBag.Admins = adminUsers.OrderBy(a => a.user.FirstName).ToList();
            ViewBag.Users = regularUsers.OrderBy(a => a.user.FirstName).ToList();
            ViewBag.Blocked = blockedUsers.OrderBy(a => a.user.FirstName).ToList();
            ViewBag.Chapters = db.Chapters.OrderByDescending(c => c.Id).ToList();
            ViewBag.Announcements = db.Announcements.OrderByDescending(c => c.Id).ToList();
            ViewBag.UnpublishedPhotos = db.GalleryPhotos.Where(p => p.Published == false && p.Ignored == false).OrderBy(p => p.Id).ToList();
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
            ViewBag.Announcements = db.Announcements.Where(a => a.ChapterId == id).Count();
            ViewBag.NewUsers = db.Users.Where(u => u.JoinChapterId == id).Count();
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
        public ActionResult CreateChapter([Bind(Include = "Id,ChapterName,ChapterYear,CurrentChapter,AddressLine1,AddressLine2,Latitude,Longitude")] Chapter chapter)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            db.Chapters.Add(chapter);
            db.SaveChanges();
            return RedirectToAction("Admin", "Home");
        }

        // Post: MakeCurrentChapter
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

        // GET: Home/EditChapter/5
        public ActionResult EditChapter(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chapter chapter = db.Chapters.Find(id);
            if (chapter == null)
            {
                return HttpNotFound();
            }
            return View(chapter);
        }

        // POST: Home/EditChapter/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditChapter([Bind(Include = "Id,ChapterName,ChapterYear,CurrentChapter,AddressLine1,AddressLine2,Latitude,Longitude")] Chapter chapter)
        {
            if (ModelState.IsValid)
            {
                db.Chapters.Attach(chapter);
                db.Entry(chapter).Property("ChapterName").IsModified = true;
                db.Entry(chapter).Property("ChapterYear").IsModified = true;
                db.Entry(chapter).Property("AddressLine1").IsModified = true;
                db.Entry(chapter).Property("AddressLine2").IsModified = true;
                db.Entry(chapter).Property("Latitude").IsModified = true;
                db.Entry(chapter).Property("Longitude").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Admin", "Home");
            }
            return View(chapter);
        }

        // Post: CreateAnnouncement
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateAnnouncement([Bind(Include = "Id,AuthorId,Title,Body,Created,ChapterId")] Announcement announcement)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            announcement.AuthorId = user.Id;
            announcement.Created = System.DateTime.Now;
            announcement.ChapterId = currentChapter.Id;
            db.Announcements.Add(announcement);
            db.SaveChanges();
            return RedirectToAction("Admin", "Home");
        }

        // GET: Home/EditAnnouncement/5
        public ActionResult EditAnnouncement(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Announcement announcement = db.Announcements.Find(id);
            if (announcement == null)
            {
                return HttpNotFound();
            }
            return View(announcement);
        }

        // POST: Home/EditAnnouncement/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAnnouncement([Bind(Include = "Id,AuthorId,Title,Body,Created,ChapterId")] Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                db.Announcements.Attach(announcement);
                db.Entry(announcement).Property("Title").IsModified = true;
                db.Entry(announcement).Property("Body").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Admin", "Home");
            }
            return View(announcement);
        }

        // GET: Home/DeleteAnnouncement/5
        public ActionResult DeleteAnnouncement(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Announcement note = db.Announcements.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        // POST: Home/DeleteAnnouncement/5
        [HttpPost, ActionName("DeleteAnnouncement")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAnnouncementConfirmed(int id)
        {
            Announcement announcement = db.Announcements.Find(id);
            db.Announcements.Remove(announcement);
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
                    myEvent.Title = eVent.Title;
                    myEvent.Description = eVent.Description;
                    myEvent.Speakers = eVent.Speakers;
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
        public ActionResult AddPhoto(GalleryPhoto galleryPhoto, HttpPostedFileBase image)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);
            if (ImageUploadValidator.IsWebFriendlyImage(image))
            {
                //Counter
                var num = 0;
                //Gets Filename without the extension
                var fileName = Path.GetFileNameWithoutExtension(image.FileName);
                var gPic = Path.Combine("/GalleryPhotos/", fileName + Path.GetExtension(image.FileName));
                //Checks if pPic matches any of the current attachments, 
                //if so it will loop and add a (number) to the end of the filename
                while (db.GalleryPhotos.Any(p => p.File == gPic))
                {
                    //Sets "filename" back to the default value
                    fileName = Path.GetFileNameWithoutExtension(image.FileName);
                    //Add's parentheses after the name with a number ex. filename(4)
                    fileName = string.Format(fileName + "(" + ++num + ")");
                    //Makes sure pPic gets updated with the new filename so it could check
                    gPic = Path.Combine("/GalleryPhotos/", fileName + Path.GetExtension(image.FileName));
                }
                image.SaveAs(Path.Combine(Server.MapPath("~/GalleryPhotos/"), fileName + Path.GetExtension(image.FileName)));
                galleryPhoto.File = gPic;
                db.SaveChanges();
            }

            galleryPhoto.Created = System.DateTime.Now;
            galleryPhoto.AuthorId = user.Id;
            galleryPhoto.Published = false;
            galleryPhoto.Ignored = false;
            galleryPhoto.ChapterId = currentChapter.Id;
            db.GalleryPhotos.Add(galleryPhoto);
            db.SaveChanges();

            return RedirectToAction("ProfilePage", "Home", new { id = User.Identity.GetUserId() });
        }

        // GET: Home/EditPhoto/5
        public ActionResult EditPhoto(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GalleryPhoto galleryPhoto = db.GalleryPhotos.Find(id);
            if (galleryPhoto == null)
            {
                return HttpNotFound();
            }
            return View(galleryPhoto);
        }

        // POST: Home/EditPhoto/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPhoto([Bind(Include = "Id,File,Caption,AuthorId,Created,Published,Ignored,ChapterId")] GalleryPhoto galleryPhoto)
        {
            if (ModelState.IsValid)
            {
                db.GalleryPhotos.Attach(galleryPhoto);
                db.Entry(galleryPhoto).Property("Caption").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("ProfilePage", "Home", new { id = User.Identity.GetUserId() });
            }
            return View(galleryPhoto);
        }

        // GET: Home/DeletePhoto/5
        public ActionResult DeletePhoto(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GalleryPhoto galleryPhoto = db.GalleryPhotos.Find(id);
            if (galleryPhoto == null)
            {
                return HttpNotFound();
            }
            return View(galleryPhoto);
        }

        // POST: Home/DeleteNote/5
        [HttpPost, ActionName("DeletePhoto")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePhotoConfirmed(int id)
        {
            GalleryPhoto galleryPhoto = db.GalleryPhotos.Find(id);
            db.GalleryPhotos.Remove(galleryPhoto);
            db.SaveChanges();
            return RedirectToAction("ProfilePage", "Home", new { id = User.Identity.GetUserId() });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public ActionResult Publish(List<int> Publish, List<int> Ignore)
        {
            if (Publish != null)
            {
                int count = Publish.Count();
                for (int i = 0; i < count; i++)
                {
                    var photo = db.GalleryPhotos.Find(Publish[i]);
                    photo.Published = true;
                    db.SaveChanges();
                }
            }

            if (Ignore != null)
            {
                int total = Ignore.Count();
                for (int i = 0; i < total; i++)
                {
                    var photo = db.GalleryPhotos.Find(Ignore[i]);
                    photo.Ignored = true;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Admin", "Home");
        }

        public ActionResult MapAndVenue()
        {
            var currentChapter = db.Chapters.First(c => c.CurrentChapter == true);

            ViewBag.CurrentChapterName = currentChapter.ChapterName;
            ViewBag.CurrentChapterYear = currentChapter.ChapterYear;
            ViewBag.CurrentAddressLine1 = currentChapter.AddressLine1;
            ViewBag.CurrentAddressLine2 = currentChapter.AddressLine2;
            ViewBag.CurrentLatitude = currentChapter.Latitude;
            ViewBag.CurrentLongitude = currentChapter.Longitude;
            ViewBag.CurrentCenter = currentChapter.Latitude + "," + currentChapter.Longitude;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendMessage(string name, string email, string subject, string message)
        {
            object returnValue = null;
            try
            {
                var mC = db.Users.FirstOrDefault(u => u.Id == "74ab1b5e-4bcd-4272-80af-bd82a31e8fbd");
                var mA = db.Users.FirstOrDefault(u => u.Id == "d3306f11-1fd5-4a4f-8f07-5d774cc590ca");
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