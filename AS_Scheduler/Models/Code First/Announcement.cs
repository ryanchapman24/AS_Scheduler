using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models.Code_First
{
    public class Announcement
    {
        public int Id { get; set; }
        public string AuthorId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public int ChapterId { get; set; }
        public int AnnouncementImageId { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public virtual Chapter Chapter { get; set; }
        public virtual AnnouncementImage AnnouncementImage { get; set; }
    }
}