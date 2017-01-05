using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models.Code_First
{
    public class Chapter
    {
        public int Id { get; set; }
        public string ChapterName { get; set; }
        public int ChapterYear { get; set; }
        public bool CurrentChapter { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}