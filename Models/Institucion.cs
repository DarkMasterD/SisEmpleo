// Archivo: Models/Institucion.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class Institucion
    {
        [Key]
        public int id_institucion { get; set; }

        public int id_pais { get; set; }
        public int id_provincia { get; set; }

        [Required(ErrorMessage = "El nombre de la institución es obligatorio.")]
        [StringLength(120)]
        public string nombre { get; set; }

        [StringLength(200)]
        public string? direccion { get; set; }

        public int? id_postulante { get; set; }

        [ForeignKey("id_pais")]
        public virtual Pais Pais { get; set; }

        [ForeignKey("id_provincia")]
        public virtual Provincia Provincia { get; set; }

        [ForeignKey("id_postulante")]
        public virtual Postulante? Postulante { get; set; }

        // Colecciones inversas (se mantienen)
        public virtual ICollection<Idioma_Curriculum> IdiomaCurriculums { get; set; } = new List<Idioma_Curriculum>();
        public virtual ICollection<FormacionAcademica> FormacionesAcademicas { get; set; } = new List<FormacionAcademica>();
        public virtual ICollection<Certificacion_Curriculum> CertificacionCurriculums { get; set; } = new List<Certificacion_Curriculum>();
    }
}