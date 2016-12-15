using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models.Code_First
{
    public class Poll
    {
        public int Id { get; set; }
        public string AuthorId { get; set; }
        public string Question { get; set; }
        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public DateTime Created { get; set; }
        public DateTime CloseDate { get; set; }

        public virtual ApplicationUser Author { get; set; }
    }
}