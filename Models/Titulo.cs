// En SisEmpleo/Models/Titulo.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Para ICollection

namespace SisEmpleo.Models
{
    public class Titulo
    {
        [Key]
        public int id_titulo { get; set; }

        public int id_especialidad { get; set; } // Tu FK
        [ForeignKey("id_especialidad")] // Apunta a la propiedad FK de arriba
        public virtual Especialidad Especialidad { get; set; } // Propiedad de navegación

        [Required]
        public string nombre { get; set; }

        public string? descripcion { get; set; }

        [Required]
        [StringLength(50)]
        public string tipo { get; set; }

        public int id_postulante { get; set; }
        [ForeignKey("id_postulante")]
        public virtual Postulante Postulante { get; set; }

        public virtual ICollection<FormacionAcademica> FormacionesAcademicas { get; set; } = new List<FormacionAcademica>();
    }
}