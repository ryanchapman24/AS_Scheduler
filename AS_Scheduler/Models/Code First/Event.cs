using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models.Code_First
{
    public class Event
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public int DayId { get; set; }
        public int ChapterId { get; set; }

        public virtual Day Day { get; set; }
        public virtual Chapter Chapter { get; set; }
    }
}