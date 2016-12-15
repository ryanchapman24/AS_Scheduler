using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models.Code_First
{
    public class Vote
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string AuthorId { get; set; }
        public bool AnsweredYes { get; set; }
        public DateTime Created { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public virtual Poll Poll { get; set; }
    }
}