using System;
using System.Collections.Generic;
using System.Text;

namespace HelpDesk.Models.ViewModels
{
    public class UpdateUserRoleViewModel
    {
        public string Id { get; set; }
        public List<string> Roles { get; set; }
    }
}
