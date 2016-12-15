using Scheduler.Models.Code_First;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models
{
    public class PhotoJudgment
    {
        public PhotoJudgment()
        {
            this.PhotoList = new List<GalleryPhoto>();
        }

        public List<GalleryPhoto> PhotoList { get; set; }
    }
}