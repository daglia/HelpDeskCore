using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{
    [Table("Failures")]
    public class Failure : BaseEntity<int>
    {
        [StringLength(100, ErrorMessage = "Arıza adı en az 3, en fazla 100 karakter içerebilir.", MinimumLength = 3)]
        [DisplayName("Arıza Adı")]
        [Required]
        public string FailureName { get; set; }
        [StringLength(100, ErrorMessage = "Model adı en az 3, en fazla 100 karakter içerebilir.", MinimumLength = 3)]
        [DisplayName("Model Adı")]
        [Required]
        public string ProductModel { get; set; }
        [StringLength(300, ErrorMessage = "Açıklama en az 20, en fazla 300 karakter içerebilir.", MinimumLength = 20)]
        [DisplayName("Açıklama")]
        [Required]
        public string Description { get; set; }

        [DisplayName("İşlem Durumu")]
        public OperationStatuses OperationStatus { get; set; } = OperationStatuses.Pending;
        [DisplayName("İşlem Zamanı")]
        public DateTime? OperationTime { get; set; }

        [DisplayName("Süreç")]
        public RepairProcesses? RepairProcess { get; set; }

        [DisplayName("Başlama Zamanı")]
        public DateTime? StartingTime { get; set; }
        [DisplayName("Bitirme Zamanı")]
        public DateTime? FinishingTime { get; set; }

        [DisplayName("Adres")]
        [StringLength(100, ErrorMessage = "Adres alanı  en az 10, en fazla 100 karakter içerebilir.", MinimumLength = 10)]
        [Required]
        public string Address { get; set; }

        [DisplayName("Enlem")]
        public double? Latitude { get; set; }
        [DisplayName("Boylam")]
        public double? Longitude { get; set; }

        [DisplayName("Fiyat")]
        public decimal Price { get; set; }
        [DisplayName("Garanti")]
        public bool HasWarranty { get; set; }
        [DisplayName("Rapor")]
        public string Report { get; set; }

        [DisplayName("Fotoğraf")]
        public ICollection<string> PhotoPath { get; set; }

        public string ClientId { get; set; }
        public string TechnicianId { get; set; }
        public string OperatorId { get; set; }
        public string SurveyId { get; set; }

        [ForeignKey("ClientId")]
        public virtual ApplicationUser Client { get; set; }
        [ForeignKey("TechnicianId")]
        public virtual ApplicationUser Technician { get; set; }
        [ForeignKey("OperatorId")]
        public virtual ApplicationUser Operator { get; set; }
        [ForeignKey("SurveyId")]
        public virtual Survey Survey { get; set; }


        public virtual ICollection<FailureLog> FailureLogs { get; set; } = new HashSet<FailureLog>();
        public virtual ICollection<Photo> Photos { get; set; } = new HashSet<Photo>();
    }
}
