using System;
using System.Collections.Generic;
using System.Text;

namespace HelpDesk.Models.ViewModels
{
    public class ProfilePasswordViewModel
    {
        public UserProfileViewModel UserProfileViewModel { get; set; }
        public ChangePasswordViewModel ChangePasswordViewModel { get; set; }
    }
}
