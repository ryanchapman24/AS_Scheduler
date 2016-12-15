using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models
{
    public class AdminUserListModels
    {
        public ApplicationUser user { get; set; }
        public List<string> roles { get; set; }
    }
}