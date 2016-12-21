using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Scheduler.Models.Code_First;
using System.Collections.Generic;

namespace Scheduler.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string ProfilePic { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public ApplicationUser()
        {
            this.MyEvents = new HashSet<MyEvent>();
            this.GalleryPhotos = new HashSet<GalleryPhoto>();
            this.Notes = new HashSet<Note>();
            this.Votes = new HashSet<Vote>();
        }

        public virtual ICollection<MyEvent> MyEvents { get; set; }
        public virtual ICollection<GalleryPhoto> GalleryPhotos { get; set; }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }

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

        //All Code First Models need a DbSet Collection reference here!
        public DbSet<MyEvent> MyEvents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Day> Days { get; set; }
        public DbSet<Announcement> Announcments { get; set; }
        public DbSet<GalleryPhoto> GalleryPhotos { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Tip> Tips { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
    }
}