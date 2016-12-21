using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models.Code_First
{
    public class GalleryPhoto
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public string File { get; set; }
        public string Caption { get; set; }
        public string AuthorId { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool Published { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public virtual Chapter Chapter { get; set; }
    }
}