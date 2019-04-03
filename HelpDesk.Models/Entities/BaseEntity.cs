using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HelpDesk.Models.Entities
{
    public abstract class BaseEntity<T> : AuditEntity
    {
        [Key]
        [Column(Order = 1)]
        public T Id { get; set; }
        public T ShallowCopy()
        {
            return (T)MemberwiseClone();
        }
        public T DeepCopy()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

    }
    public abstract class AuditEntity
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string CreatedUserId { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(450)]
        public string UpdatedUserId { get; set; }
    }
}
