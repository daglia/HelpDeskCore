using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HelpDesk.Models.Enums;

namespace HelpDesk.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Ad")]
        [StringLength(25)]
        public string Name { get; set; }
        [StringLength(35)]
        [Required]
        [Display(Name = "Soyad")]
        public string Surname { get; set; }
        [Required]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; }

        // Teknisyene özel
        [Display(Name = "Enlem")]
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [Display(Name = "Boylam")]
        public TechnicianStatuses? TechnicianStatus { get; set; }

        public string AvatarPath { get; set; }
        //[Display(Name = "Kullanıcı Avatarı")]
        //public HttpPostedFileBase PostedFile { get; set; }
    }
}
