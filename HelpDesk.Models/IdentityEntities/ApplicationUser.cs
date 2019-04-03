using HelpDesk.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using HelpDesk.Models.Entities;

namespace HelpDesk.Models.IdentityEntities
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        [StringLength(60)]
        [Required]
        public string Surname { get; set; }

        public string Phone { get; set; }

        public string ActivationCode { get; set; }
        public string AvatarPath { get; set; }

        // Teknisyene özel

        public TechnicianStatuses? TechnicianStatus { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
