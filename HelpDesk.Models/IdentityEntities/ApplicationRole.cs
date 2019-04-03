using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.IdentityEntities
{
    public class ApplicationRole : IdentityRole
    {
        [StringLength(200)]
        public string Description { get; set; }
    }
}
