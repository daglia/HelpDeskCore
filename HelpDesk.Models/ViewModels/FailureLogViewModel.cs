using HelpDesk.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelpDesk.Models.ViewModels
{
    public class FailureLogViewModel
    {
        public string Message { get; set; }
        public IdentityRoles FromWhom { get; set; }
        public DateTime CreatedDate { get; set; }

        public int FailureId { get; set; }
    }
}
