using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models.Code_First
{
    public class MyEvent
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Speakers { get; set; }
        public int DayId { get; set; }
        public int ChapterId { get; set; }
        public string AuthorId { get; set; }
        public int CorrespondingEventId { get; set; }

        public virtual Day Day { get; set; }
        public virtual Chapter Chapter { get; set; }
        public virtual ApplicationUser Author { get; set; }
    }
}