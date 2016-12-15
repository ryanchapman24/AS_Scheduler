using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Scheduler.Models
{
    public class AdminUserViewModels
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public MultiSelectList Roles { get; set; }
        public string[] SelectedRoles { get; set; }
    }
}