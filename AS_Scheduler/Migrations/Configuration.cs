namespace Scheduler.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Scheduler.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Scheduler.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Administrator"))
            {
                roleManager.Create(new IdentityRole { Name = "Administrator" });
            }

            if (!context.Roles.Any(r => r.Name == "User"))
            {
                roleManager.Create(new IdentityRole { Name = "User" });
            }

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "your email address"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "rchapman@anomalysquared.com",
                    Email = "rchapman@anomalysquared.com",
                    FirstName = "Ryan",
                    LastName = "Chapman",
                    DisplayName = "Ryan Chapman",
                    PhoneNumber = "(919) 698-2849",
                    Company = "Anomaly Squared",
                    JobTitle = ".NET Developer",
                    ProfilePic = "/ProfilePics/Headshot_Square.png",
                    JoinChapterId = 1
                }, "Chappy24!");
            }

            var userId_Super = userManager.FindByEmail("rchapman@anomalysquared.com").Id;
            userManager.AddToRole(userId_Super, "Administrator");
            userManager.AddToRole(userId_Super, "User");

            if (!context.Users.Any(u => u.Email == "your email address"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "tedwards@anomalysquared.com",
                    Email = "tedwards@anomalysquared.com",
                    FirstName = "Taylor",
                    LastName = "Edwards",
                    DisplayName = "Taylor Edwards",
                    PhoneNumber = "(336) 790-8425",
                    Company = "Anomaly Squared",
                    JobTitle = "Marketing Coordinator",
                    ProfilePic = "/assets/images/DefaultProfilePic.png",
                    JoinChapterId = 1,
                }, "Password1!");
            }

            var userId_Super2 = userManager.FindByEmail("tedwards@anomalysquared.com").Id;
            userManager.AddToRole(userId_Super2, "Administrator");
            userManager.AddToRole(userId_Super2, "User");


            if (!context.Users.Any(u => u.Email == "your email address"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "cgotsch@anomalysquared.com",
                    Email = "cgotsch@anomalysquared.com",
                    FirstName = "Clarissa",
                    LastName = "Gotsch",
                    DisplayName = "Clarissa Gotsch",
                    PhoneNumber = "(919) 741-8834",
                    Company = "Anomaly Squared",
                    JobTitle = "Marketing Assistant",
                    ProfilePic = "/assets/images/DefaultProfilePic.png",
                    JoinChapterId = 1
                }, "Password1!");
            }

            var userId_Super3 = userManager.FindByEmail("cgotsch@anomalysquared.com").Id;
            userManager.AddToRole(userId_Super3, "Administrator");
            userManager.AddToRole(userId_Super3, "User");
        }
    }
}
