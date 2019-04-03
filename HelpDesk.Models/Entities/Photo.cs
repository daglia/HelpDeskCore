using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{
    public class Photo : BaseEntity<string>
    {
        public Photo()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Required]
        [DisplayName("Yol")]
        public string Path { get; set; }

        public int FailureId { get; set; }

        [ForeignKey("FailureId")]
        public virtual Failure Failure { get; set; }
    }
}
